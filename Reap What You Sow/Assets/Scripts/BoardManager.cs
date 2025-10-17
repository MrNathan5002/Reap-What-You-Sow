using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Refs")]
    public GridManager grid;                 // assign your existing GridManager
    public GameObject cropPrefab;            // simple sprite prefab with CropInstance
    public Transform cropsParent;            // where placed crops live

    private CropInstance[,] crops;           // same dims as grid

    void Awake()
    {
        if (!grid) grid = FindObjectOfType<GridManager>();
        if (!cropsParent)
        {
            var go = new GameObject("__Crops");
            go.transform.SetParent(transform, false);
            cropsParent = go.transform;
        }
        crops = new CropInstance[grid.Width, grid.Height];
    }

    public bool InBounds(Vector2Int c) => grid.InBounds(c);

    public bool IsEmpty(Vector2Int c)
    {
        if (!InBounds(c)) return false;
        return crops[c.x, c.y] == null;
    }

    public bool CanPlace(Vector2Int c) => InBounds(c) && IsEmpty(c);

    public bool PlaceCrop(Vector2Int cell, bool upgraded, int lifetime, int baseCandy, Sprite sprite = null)
    {
        if (!CanPlace(cell)) return false;

        var wpos = grid.GridToWorld(cell);
        var go = Instantiate(cropPrefab, wpos, Quaternion.identity, cropsParent);
        var ci = go.GetComponent<CropInstance>();
        if (!ci) ci = go.AddComponent<CropInstance>();
        ci.Init(cell, upgraded, lifetime, baseCandy);

        // Optional: set sprite based on card
        if (sprite)
        {
            var sr = go.GetComponent<SpriteRenderer>();
            if (!sr) sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
        }

        crops[cell.x, cell.y] = ci;
        return true;
    }

    /// <summary>Resolve one round; returns candy gained this round.</summary>
    public int ResolveRound(bool includeDiagonals = true, bool isTrickRound = false)
    {
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

                // Base treat yield for now. (We’ll plug real Trick effects next step.)
                int roundYield = c.baseCandyPerRound;

                // Example adjacency hook (disabled by default):
                // int neighbors = CountNeighbors(new Vector2Int(x, y), dirs);
                // roundYield += neighbors; // or multiply etc.

                gained += Mathf.Max(0, roundYield);

                // Lifetime tick down
                c.lifetime -= 1;
                if (c.lifetime <= 0) toRemove.Add(c);
            }

        // Remove expired crops
        foreach (var dead in toRemove)
        {
            crops[dead.cell.x, dead.cell.y] = null;
            Destroy(dead.gameObject);
        }

        return gained;
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
}
