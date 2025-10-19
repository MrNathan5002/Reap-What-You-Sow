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
        if (!deck || !board || !grid || !hand) return;

        var hc = GetComponent<HandCard>();
        var disp = GetComponent<CardDisplay>();
        if (hc == null || hc.instance == null || disp == null || disp.cardData == null) return;

        var def = disp.cardData;
        bool upgraded = hc.instance.isUpgraded;


        int cost = upgraded ? def.upgradedEnergy : def.baseEnergy;
        int lifetime = upgraded ? def.upgradedLifetime : def.baseLifetime;
        int treatY = upgraded ? def.upgradedTreatCandy : def.baseTreatCandy;
        int trickY = upgraded ? def.upgradedTrickCandy : def.baseTrickCandy;

        // Convert mouse to grid cell
        var cam = Camera.main;
        Vector3 sp = eventData.position;
        float z = Mathf.Abs(grid.transform.position.z - cam.transform.position.z);
        var world = cam.ScreenToWorldPoint(new Vector3(sp.x, sp.y, z));
        var cell = grid.WorldToGrid(world);

        // --- SPELLS ---
        if (def.isSpell)
        {
            if (!deck.CanAfford(cost)) return; // cost still comes from base/upgraded energy

            bool consumed = false;

            switch (def.spellKind)
            {
                case CardEditor.SpellKind.RemoveTargetCrop:
                    {
                        // remove crop under the cursor if present
                        if (board.RemoveCropAt(cell))
                        {
                            deck.SpendEnergy(cost);
                            consumed = true;
                        }
                        break;
                    }

                case CardEditor.SpellKind.GainEnergy:
                    {
                        deck.SpendEnergy(cost);
                        deck.GainEnergy(def.spellAmount); // e.g., +1
                        consumed = true;
                        break;
                    }

                default:
                    // unknown spell kind: do nothing
                    break;
            }

            if (consumed)
            {
                // Discard this specific instance and remove the visual
                deck.DiscardInstanceFromHand(hc.instance.instanceId);
                hand.RemoveCardGO(gameObject);
            }
            return; // spells don't place crops
        }

        // --- CROPS (existing behavior) ---
        if (!board.CanPlace(cell)) return;
        if (!deck.CanAfford(cost)) return;

        // Place crop using card def (Board will use def.cropSprite)
        bool placed = board.PlaceCropFromDef(cell, def, upgraded, lifetime, /*sprite:*/ null);
        if (!placed) return;

        // Spend energy and discard this specific instance
        deck.SpendEnergy(cost);
        deck.DiscardInstanceFromHand(hc.instance.instanceId);

        // Remove visual
        hand.RemoveCardGO(gameObject);
    }
}


