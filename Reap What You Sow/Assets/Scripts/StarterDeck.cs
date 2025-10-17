using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StarterDeck", menuName = "Cards/Starter Deck")]
public class StarterDeck : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public CardEditor def;
        public int count;
        public bool startUpgraded;
    }

    public List<Entry> entries = new List<Entry>();

    [Header("Starting Player Stats")]
    public int startHandSize = 5;
    public int startEnergyMax = 3;

    [Header("Night Settings")]
    public int roundsPerNight = 5;
    public int quotaCandy = 40;
}
