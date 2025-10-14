using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField, Min(1)] int width = 5;
    [SerializeField, Min(1)] int height = 5;
    [SerializeField, Min(0.1f)] float cellSize = 1f;
    [SerializeField] Vector2 origin = Vector2.zero; // bottom-left world-space origin

    [Header("Tile Prefab")]
    [SerializeField] GameObject tilePrefab; // must have SpriteRenderer (and GridTile will be added)

    [Header("Hierarchy")]
    [SerializeField] Transform tilesParent;

    // Backing store
    readonly List<GridTile> tiles = new();
    GridTile[,] grid; // [x,y]

    public int Width => width;
    public int Height => height;
    public float CellSize => cellSize;
    public Vector2 Origin => origin;

    void OnValidate()
    {
        width = Mathf.Max(1, width);
        height = Mathf.Max(1, height);
        cellSize = Mathf.Max(0.1f, cellSize);
    }

    void Awake()
    {
        if (Application.isPlaying) return;
        // In Edit Mode keep parent reference tidy
        if (!tilesParent)
        {
            var t = transform.Find("__Tiles");
            if (t) tilesParent = t;
        }
    }

    void Start()
    {
        // Build at runtime automatically
#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif
        Rebuild();
    }

    [ContextMenu("Rebuild Grid")]
    public void Rebuild()
    {
        if (!tilePrefab)
        {
            Debug.LogError("[GridManager] Assign a Tile Prefab.");
            return;
        }

        ClearExisting();

        grid = new GridTile[width, height];

        if (!tilesParent)
        {
            var go = new GameObject("__Tiles");
            go.transform.SetParent(transform, false);
            tilesParent = go.transform;
        }

        // Build grid bottom-left (0,0) to top-right (width-1, height-1)
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 wpos = GridToWorld(new Vector2Int(x, y));
                var tileGO = Instantiate(tilePrefab, wpos, Quaternion.identity, tilesParent);

                // Scale tile to cell size (assumes sprite size of 1 world unit)
                tileGO.transform.localScale = Vector3.one * cellSize;

                // Ensure GridTile exists
                var tile = tileGO.GetComponent<GridTile>();
                if (!tile) tile = tileGO.AddComponent<GridTile>();

                // Cache SpriteRenderer if present
                tileGO.TryGetComponent(out SpriteRenderer sr);
                tile.Init(new Vector2Int(x, y), sr);

                grid[x, y] = tile;
                tiles.Add(tile);
            }
        }
    }

    void ClearExisting()
    {
        // Destroy previous runtime children
        if (tilesParent)
        {
            // Safe destroy in edit/runtime
            for (int i = tilesParent.childCount - 1; i >= 0; i--)
            {
                var child = tilesParent.GetChild(i);
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(child.gameObject);
                else
                    Destroy(child.gameObject);
#else
                Destroy(child.gameObject);
#endif
            }
        }
        tiles.Clear();
        grid = null;
    }

    // --- Public API ---

    public bool InBounds(Vector2Int c)
        => c.x >= 0 && c.x < width && c.y >= 0 && c.y < height;

    public GridTile GetTile(Vector2Int c)
        => (grid != null && InBounds(c)) ? grid[c.x, c.y] : null;

    public Vector3 GridToWorld(Vector2Int c)
        => new Vector3(origin.x + (c.x + 0.5f) * cellSize,
                       origin.y + (c.y + 0.5f) * cellSize,
                       0f);

    public Vector2Int WorldToGrid(Vector3 world)
    {
        int x = Mathf.FloorToInt((world.x - origin.x) / cellSize);
        int y = Mathf.FloorToInt((world.y - origin.y) / cellSize);
        return new Vector2Int(x, y);
    }

    public IEnumerable<GridTile> GetNeighbors(Vector2Int c, bool includeDiagonals = false)
    {
        // Orthogonals
        var dirs = new List<Vector2Int> { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        if (includeDiagonals)
        {
            dirs.Add(new Vector2Int(1, 1));
            dirs.Add(new Vector2Int(1, -1));
            dirs.Add(new Vector2Int(-1, -1));
            dirs.Add(new Vector2Int(-1, 1));
        }

        foreach (var d in dirs)
        {
            var n = c + d;
            if (InBounds(n)) yield return grid[n.x, n.y];
        }
    }

    // Convenience: highlight all tiles off/on (useful for quick visual checks)
    public void SetAllHighlights(bool on)
    {
        foreach (var t in tiles) t.SetHighlight(on);
    }

    // --- Editor Gizmos ---

    void OnDrawGizmosSelected()
    {
        // Draw outline of grid footprint and cells
        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.color = new Color(0f, 0f, 0f, 0.25f);

        // Outer border
        Vector3 bl = new Vector3(origin.x, origin.y, 0f);
        Vector3 tr = new Vector3(origin.x + width * cellSize, origin.y + height * cellSize, 0f);
        Gizmos.DrawWireCube((bl + tr) * 0.5f, new Vector3(width * cellSize, height * cellSize, 0f));

        // Cell lines
        for (int x = 1; x < width; x++)
        {
            float wx = origin.x + x * cellSize;
            Gizmos.DrawLine(new Vector3(wx, origin.y, 0f), new Vector3(wx, origin.y + height * cellSize, 0f));
        }
        for (int y = 1; y < height; y++)
        {
            float wy = origin.y + y * cellSize;
            Gizmos.DrawLine(new Vector3(origin.x, wy, 0f), new Vector3(origin.x + width * cellSize, wy, 0f));
        }
    }
}
