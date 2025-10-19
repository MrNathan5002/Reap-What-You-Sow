using UnityEngine;

[CreateAssetMenu(fileName = "NewCardPack", menuName = "RWYS/Card Pack")]
public class CardPack : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public CardEditor def;
        public int count;
        public bool upgraded;
    }

    public string packName = "Pack";
    public Entry[] entries;

    public void AddAllToDeck(DeckManager deck)
    {
        if (!deck || entries == null) return;
        foreach (var e in entries)
        {
            if (!e.def || e.count <= 0) continue;
            deck.AddCard(e.def, e.count, e.upgraded);
        }
    }
}

