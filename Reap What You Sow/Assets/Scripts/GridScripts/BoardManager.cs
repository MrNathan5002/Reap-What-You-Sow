using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Refs")]
    public GridManager grid;                 // assign your existing GridManager
    public GameObject cropPrefab;            // prefab with SpriteRenderer (and optionally CropInstance)
    public Transform cropsParent;            // where placed crops live

    private CropInstance[,] crops;           // [x,y] storage

    void Awake()
    {
        if (!grid) grid = FindObjectOfType<GridManager>();

        if (!cropsParent)
        {
            var go = new GameObject("__Crops");
            go.transform.SetParent(transform, false);
            cropsParent = go.transform;
        }

        EnsureStorage();
    }

    // Recreate storage if grid is missing or size changed (call before any access)
    void EnsureStorage()
    {
        if (!grid) return;

        if (crops == null ||
            crops.GetLength(0) != grid.Width ||
            crops.GetLength(1) != grid.Height)
        {
            crops = new CropInstance[grid.Width, grid.Height];
        }
    }

    public bool InBounds(Vector2Int c)
    {
        EnsureStorage();
        return grid && grid.InBounds(c);
    }

    public bool IsEmpty(Vector2Int c)
    {
        EnsureStorage();
        if (!InBounds(c)) return false;
        return crops[c.x, c.y] == null;
    }

    public bool CanPlace(Vector2Int c) => InBounds(c) && IsEmpty(c);

    /// <summary>Place a crop with per-round Treat/Trick yields.</summary>
    public bool PlaceCropFromDef(Vector2Int cell, CardEditor def, bool upgraded, int lifetime, Sprite sprite = null)
    {
        EnsureStorage();
        if (!CanPlace(cell) || !grid || !cropPrefab || !def) return false;

        Vector3 wpos = grid.GridToWorld(cell);
        var go = Instantiate(cropPrefab, wpos, Quaternion.identity, cropsParent);

        var ci = go.GetComponent<CropInstance>() ?? go.AddComponent<CropInstance>();
        ci.Init(def, cell, upgraded, lifetime, 0, 0);

        // NEW: prefer the sprite argument, otherwise use def.cropSprite
        var sr = go.GetComponent<SpriteRenderer>() ?? go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite != null ? sprite : def.cropSprite;

        crops[cell.x, cell.y] = ci;
        return true;
    }


    /// <summary>Resolve one round; returns total candy gained this round.</summary>
    public int ResolveRound(bool includeDiagonals = true, bool isTrickRound = false)
    {
        EnsureStorage();
        if (!grid) return 0;

        int gained = 0;
        var toRemove = new List<CropInstance>();

        var dirs = new List<Vector2Int> { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        if (includeDiagonals)
        {
            dirs.Add(new Vector2Int(1, 1));
            dirs.Add(new Vector2Int(1, -1));
            dirs.Add(new Vector2Int(-1, -1));
            dirs.Add(new Vector2Int(-1, 1));
        }

        int W = grid.Width, H = grid.Height;

        // --- Pass 1: neighbor effects landing on each cell ---
        bool[,] flip = new bool[W, H];   // if true, that cell flips Treat<->Trick for this round
        int[,] aura = new int[W, H];    // additive yield from neighbors

        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                var src = crops[x, y];
                if (!src || !src.def) continue;

                // Provider uses the GLOBAL round to decide which neighbor effect it emits
                bool srcIsTrick = isTrickRound;

                bool doFlip = srcIsTrick ? src.def.trickFlipNeighbors : src.def.treatFlipNeighbors;
                int addAura = srcIsTrick ? src.def.trickAuraToNeighbors : src.def.treatAuraToNeighbors;

                if (!doFlip && addAura == 0) continue;

                foreach (var d in dirs)
                {
                    var n = new Vector2Int(x, y) + d;
                    if (!InBounds(n)) continue;
                    if (crops[n.x, n.y] == null) continue;

                    if (doFlip) flip[n.x, n.y] = true;      // any source can set flip
                    if (addAura != 0) aura[n.x, n.y] += addAura;
                }
            }

        // --- Pass 2: compute local yields with possible flip + adjacency + aura ---
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                var c = crops[x, y];
                if (!c || !c.def) continue;

                // Local mode: global round XOR flip-on-this-cell
                bool localIsTrick = isTrickRound ^ flip[x, y];

                // Base & adjacency per neighbor (you already store both)
                int baseTreat = c.isUpgraded ? c.def.upgradedTreatCandy : c.def.baseTreatCandy;
                int baseTrick = c.isUpgraded ? c.def.upgradedTrickCandy : c.def.baseTrickCandy;

                int adjTreat = c.isUpgraded ? c.def.upgradedTreatAdjPerNeighbor : c.def.baseTreatAdjPerNeighbor;
                int adjTrick = c.isUpgraded ? c.def.upgradedTrickAdjPerNeighbor : c.def.baseTrickAdjPerNeighbor;

                // neighbors (diagonals included if set)
                int neighbors = CountNeighbors(new Vector2Int(x, y), dirs);

                int baseYield = localIsTrick ? baseTrick : baseTreat;
                int adjPer = localIsTrick ? adjTrick : adjTreat;

                int roundYield = baseYield + neighbors * adjPer;

                // add aura from neighbors landing on this cell
                roundYield += aura[x, y];

                // clamp to non-negative
                if (roundYield < 0) roundYield = 0;

                gained += roundYield;

                // lifetime tick
                c.lifetime -= 1;
                if (c.lifetime <= 0) toRemove.Add(c);
            }

        // cleanup expired
        foreach (var dead in toRemove)
        {
            crops[dead.cell.x, dead.cell.y] = null;
            if (dead) Destroy(dead.gameObject);
        }

        return Mathf.Max(0, gained);
    }


    int CountNeighbors(Vector2Int cell, List<Vector2Int> dirs)
    {
        int count = 0;
        foreach (var d in dirs)
        {
            var n = cell + d;
            if (InBounds(n) && crops[n.x, n.y] != null) count++;
        }
        return count;
    }

    public void ClearAllCrops()
    {
        EnsureStorage();
        for (int y = 0; y < crops.GetLength(1); y++)
            for (int x = 0; x < crops.GetLength(0); x++)
            {
                if (crops[x, y])
                {
                    Destroy(crops[x, y].gameObject);
                    crops[x, y] = null;
                }
            }
    }

    public bool HasCropAt(Vector2Int cell)
    {
        EnsureStorage();
        if (!InBounds(cell)) return false;
        return crops[cell.x, cell.y] != null;
    }

    public bool RemoveCropAt(Vector2Int cell)
    {
        EnsureStorage();
        if (!InBounds(cell)) return false;
        var c = crops[cell.x, cell.y];
        if (!c) return false;
        crops[cell.x, cell.y] = null;
        Destroy(c.gameObject);
        return true;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!grid || crops == null) return;
        Gizmos.color = new Color(1f, 1f, 0f, 0.15f);
        for (int y = 0; y < Mathf.Min(crops.GetLength(1), grid.Height); y++)
            for (int x = 0; x < Mathf.Min(crops.GetLength(0), grid.Width); x++)
            {
                if (crops[x, y] == null) continue;
                Gizmos.DrawCube(grid.GridToWorld(new Vector2Int(x, y)), Vector3.one * grid.CellSize * 0.6f);
            }
    }
#endif
}
