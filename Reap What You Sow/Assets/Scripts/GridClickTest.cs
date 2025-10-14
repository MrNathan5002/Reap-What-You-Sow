// TEMP ONLY: click to toggle highlight on the tile under the mouse.
using UnityEngine;

public class GridClickTest : MonoBehaviour
{
    [SerializeField] GridManager grid;

    void Update()
    {
        if (!grid) return;

        if (Input.GetMouseButtonDown(0))
        {
            var world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var coord = grid.WorldToGrid(world);
            var tile = grid.GetTile(coord);
            if (tile) tile.SetHighlight(true);
        }
    }
}
