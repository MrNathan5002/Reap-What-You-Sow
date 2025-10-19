using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Card")]
public class CardEditor : ScriptableObject
{
    public string id;
    public string displayName;

    // Gameplay base values
    public int baseEnergy = 1;
    public int baseLifetime = 3;
    public int baseTreatCandy = 2;
    public int baseTrickCandy = 0;   // can be negative/positive/zero

    // One tailored upgrade per card
    public bool hasUpgrade = false;
    public int upgradedEnergy = 1;       // if unset, just copy base in Inspector
    public int upgradedLifetime = 3;
    public int upgradedTreatCandy = 3;
    public int upgradedTrickCandy = 1;

    [Header("Adjacency (per neighbor)")]
    public int baseTreatAdjPerNeighbor = 0;   // e.g., +1 candy per neighbor on Treat
    public int baseTrickAdjPerNeighbor = 0;   // e.g., -1 candy per neighbor on Trick

    [Header("Adjacency (upgraded)")]
    public int upgradedTreatAdjPerNeighbor = 0;
    public int upgradedTrickAdjPerNeighbor = 0;

    [Header("Neighbor Effects (per round mode)")]
    public bool treatFlipNeighbors = false;   // if true, neighbors invert Treat<->Trick this round
    public bool trickFlipNeighbors = false;

    public int treatAuraToNeighbors = 0;      // add to neighbors' yield (this round)
    public int trickAuraToNeighbors = 0;

    // --- Spells (optional) ---
    [Header("Spell (optional)")]
    public bool isSpell = false;

    public enum SpellKind { None, RemoveTargetCrop, GainEnergy }
    public SpellKind spellKind = SpellKind.None;

    // Used by GainEnergy (e.g., +1). Ignored for RemoveTargetCrop.
    public int spellAmount = 0;


    public Sprite cropSprite;
    public Sprite cardSprite;
}

