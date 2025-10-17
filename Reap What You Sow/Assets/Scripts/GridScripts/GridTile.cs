using UnityEngine;

[DisallowMultipleComponent]
public class GridTile : MonoBehaviour
{
    [field: SerializeField, Tooltip("Grid coordinate (x,y) assigned by GridManager.")]
    public Vector2Int Coord { get; private set; }

    [SerializeField] SpriteRenderer spriteRenderer;

    //flag for game logic later (placement, occupancy, etc.)
    public bool Occupied { get; set; }

    public void Init(Vector2Int coord, SpriteRenderer sr = null)
    {
        Coord = coord;
        if (sr != null) spriteRenderer = sr;
        if (spriteRenderer == null) TryGetComponent(out spriteRenderer);
        name = $"Tile_{coord.x}_{coord.y}";
    }

    // Simple visual hook you can call from tests/tools.
    public void SetHighlight(bool on)
    {
        if (!spriteRenderer) return;
        spriteRenderer.color = on ? new Color(0.9f, 0.9f, 1f, 1f) : Color.white;
    }
}