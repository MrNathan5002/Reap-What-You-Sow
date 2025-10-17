using UnityEngine;

public class CropInstance : MonoBehaviour
{
    // Runtime state
    public Vector2Int cell;
    public bool isUpgraded;
    public int lifetime;            // rounds remaining
    public int baseCandyPerRound;   // treat yield (we’ll add trick later)

    // Optional: sprite or highlight references could live here

    public void Init(Vector2Int cell, bool upgraded, int lifetime, int baseCandy)
    {
        this.cell = cell;
        this.isUpgraded = upgraded;
        this.lifetime = lifetime;
        this.baseCandyPerRound = baseCandy;
    }
}
