using UnityEngine;

public class EndTurnButton : MonoBehaviour
{
    public DeckManager deck;

    public void OnEndTurn()
    {
        if (!deck) deck = FindObjectOfType<DeckManager>();
        deck?.EndTurn();
    }
}
