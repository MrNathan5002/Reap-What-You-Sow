using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CardDisplay))]
public class CardPlayable : MonoBehaviour, IPointerUpHandler
{
    [Header("Gameplay Defaults (override later with data)")]
    public int energyCost = 1;
    public int lifetime = 3;          // rounds the crop lives
    public int baseCandyPerRound = 2; // treat yield for now
    public Sprite cropSprite;         // sprite to show on board (optional)

    [Header("Refs (auto-find if empty)")]
    public DeckManager deck;
    public BoardManager board;
    public GridManager grid;
    public HandManager hand;

    CardDisplay display;

    void Awake()
    {
        display = GetComponent<CardDisplay>();
        if (!deck) deck = FindObjectOfType<DeckManager>();
        if (!board) board = FindObjectOfType<BoardManager>();
        if (!grid) grid = FindObjectOfType<GridManager>();
        if (!hand) hand = FindObjectOfType<HandManager>();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!deck || !board || !grid || !hand || display == null || display.cardData == null) return;

        // Convert mouse to the grid cell under cursor
        var cam = Camera.main;
        Vector3 sp = eventData.position;
        float z = Mathf.Abs(grid.transform.position.z - cam.transform.position.z);
        var world = cam.ScreenToWorldPoint(new Vector3(sp.x, sp.y, z));
        var cell = grid.WorldToGrid(world);

        // Validate
        if (!board.CanPlace(cell))
            return;

        if (!deck.CanAfford(energyCost))
            return;

        // Place crop
        bool placed = board.PlaceCrop(cell, /*upgraded:*/ false, lifetime, baseCandyPerRound, cropSprite);
        if (!placed) return;

        // Spend energy and discard this card (by def) from hand
        deck.SpendEnergy(energyCost);
        deck.RemoveOneByDefFromHand(display.cardData);

        // Remove the visual card GO from the hand
        hand.RemoveCardGO(gameObject);
    }
}

