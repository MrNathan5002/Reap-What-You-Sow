using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public List<CardEditor> allCards = new List<CardEditor>();

    private int currentIndex = 0;

    void Start(){
        CardEditor[] cards = Resources.LoadAll<CardEditor>("cards");

        allCards.AddRange(cards);
    }

    public void DrawCard(HandManager handManager) {
        if (allCards.Count == 0)
            return;

        CardEditor nextCard = allCards[currentIndex];
        handManager.AddCardToHand(nextCard);
        currentIndex = (currentIndex +1) % allCards.Count;
    }
    
}
