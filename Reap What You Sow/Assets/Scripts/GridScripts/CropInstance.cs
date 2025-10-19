using UnityEngine;

public class CropInstance : MonoBehaviour
{
    public CardEditor def;          // NEW: reference to the card definition
    public Vector2Int cell;
    public bool isUpgraded;
    public int lifetime;
    // keep these if you want, but they’re no longer required for math:
    public int treatCandyPerRound;
    public int trickCandyPerRound;

    public void Init(CardEditor def, Vector2Int cell, bool upgraded, int lifetime, int treat, int trick)
    {
        this.def = def;                    // NEW
        this.cell = cell;
        this.isUpgraded = upgraded;
        this.lifetime = lifetime;
        this.treatCandyPerRound = treat;   // optional (we won’t rely on them)
        this.trickCandyPerRound = trick;   // optional
    }
}
