using UnityEngine;

public class CropInstance : MonoBehaviour
{
    public Vector2Int cell;
    public bool isUpgraded;
    public int lifetime;
    public int treatCandyPerRound;
    public int trickCandyPerRound;

    public void Init(Vector2Int cell, bool upgraded, int lifetime, int treat, int trick)
    {
        this.cell = cell;
        this.isUpgraded = upgraded;
        this.lifetime = lifetime;
        this.treatCandyPerRound = treat;
        this.trickCandyPerRound = trick;
    }
}
