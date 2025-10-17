using UnityEngine;

public class HoverPreview : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] GridManager grid;
    [SerializeField] GameObject ghostPrefab;

    [Header("Behavior")]
    [SerializeField] bool hideWhenOutOfBounds = true;
    [SerializeField] bool hideOnOccupied = false;         // optional: don't show if tile is occupied
    [SerializeField] Vector2 worldOffset = Vector2.zero;  // e.g., small nudge if your sprite needs it

    [Header("Pixel Art")]
    [SerializeField] bool snapToPixelGrid = true;
    [SerializeField] int pixelsPerUnit = 16;

    GameObject ghost;
    Vector2Int lastCoord = new(-999, -999);
    float Pixel => 1f / Mathf.Max(1, pixelsPerUnit);

    void Start()
    {
        if (!grid || !ghostPrefab)
        {
            Debug.LogError("[HoverPreview] Assign Grid and Ghost Prefab.");
            enabled = false; return;
        }
        ghost = Instantiate(ghostPrefab, transform);
        ghost.SetActive(false);
    }

    void Update()
    {
        if (!grid) return;

        // Mouse → world → grid coord
        var world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var coord = grid.WorldToGrid(world);

        // Out of bounds?
        bool inBounds = grid.InBounds(coord);
        if (!inBounds)
        {
            if (hideWhenOutOfBounds) ghost.SetActive(false);
            lastCoord = new(-999, -999);
            return;
        }

        // Optional: hide when tile is occupied
        if (hideOnOccupied)
        {
            var tile = grid.GetTile(coord);
            if (tile != null && tile.Occupied)
            {
                ghost.SetActive(false);
                return;
            }
        }

        // Move ghost only if coord changed
        if (coord != lastCoord)
        {
            Vector3 pos = grid.GridToWorld(coord) + (Vector3)worldOffset;
            if (snapToPixelGrid)
                pos = SnapToPixel(pos);

            ghost.transform.position = pos;
            if (!ghost.activeSelf) ghost.SetActive(true);
            lastCoord = coord;
        }
    }

    Vector3 SnapToPixel(Vector3 p)
    {
        p.x = Mathf.Round(p.x / Pixel) * Pixel;
        p.y = Mathf.Round(p.y / Pixel) * Pixel;
        return p;
    }

    // Optional: swap the ghost sprite at runtime (e.g., different card previews)
    public void SetGhostSprite(Sprite s)
    {
        if (!ghost) return;
        if (ghost.TryGetComponent<SpriteRenderer>(out var sr))
            sr.sprite = s;
    }

    // Optional: toggle externally
    public void SetVisible(bool on)
    {
        if (ghost) ghost.SetActive(on);
    }
}
