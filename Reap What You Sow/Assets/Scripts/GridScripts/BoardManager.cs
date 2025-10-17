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
    public bool PlaceCropAdvanced(Vector2Int cell, bool upgraded, int lifetime, int treat, int trick, Sprite sprite = null)
    {
        EnsureStorage();
        if (!CanPlace(cell) || !grid || !cropPrefab) return false;

        Vector3 wpos = grid.GridToWorld(cell);
        //place after this line to add sprites or sound effects


        var go = Instantiate(cropPrefab, wpos, Quaternion.identity, cropsParent);

        var ci = go.GetComponent<CropInstance>();
        if (!ci) ci = go.AddComponent<CropInstance>();
        ci.Init(cell, upgraded, lifetime, treat, trick);

        if (sprite)
        {
            var sr = go.GetComponent<SpriteRenderer>() ?? go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
        }

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

        for (int y = 0; y < grid.Height; y++)
            for (int x = 0; x < grid.Width; x++)
            {
                var c = crops[x, y];
                if (!c) continue;

                int baseYield = isTrickRound ? c.trickCandyPerRound : c.treatCandyPerRound;

                // Example adjacency hook (disabled by default):
                // int neighbors = CountNeighbors(new Vector2Int(x, y), dirs);
                // baseYield += neighbors;

                gained += baseYield;

                c.lifetime -= 1;
                if (c.lifetime <= 0) toRemove.Add(c);
            }

        // Remove expired crops
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
