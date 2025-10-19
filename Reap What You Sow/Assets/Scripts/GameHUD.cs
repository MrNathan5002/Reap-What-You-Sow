using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameHUD : MonoBehaviour
{
    [Header("Refs")]
    public DeckManager deck;                 // assign in Inspector

    [Header("Texts")]
    public TMP_Text energyText;
    public TMP_Text candyText;
    public TMP_Text roundText;                   // e.g., "Round 2/5"
    public TMP_Text modeText;                    // "TREAT" or "TRICK"
    public TMP_Text nightText;  // assign in Inspector

    [Header("Optional: Visuals")]
    public EnergyPips energyPips;            // optional pip row (below)
    public Color treatColor = new Color(0.9f, 0.95f, 1f);
    public Color trickColor = new Color(1f, 0.85f, 0.85f);

    void Awake()
    {
        if (!deck) deck = FindObjectOfType<DeckManager>();
    }

    void OnEnable()
    {
        if (!deck) return;
        deck.OnEnergyChanged += HandleEnergyChanged;
        deck.OnCandyChanged += HandleCandyChanged;
        deck.OnRoundStarted += HandleRoundStarted;
        deck.OnNightEnded += HandleNightEnded;

        // Seed initial UI if deck already initialized
        HandleEnergyChanged(deck.Energy, deck.EnergyMax);
        HandleCandyChanged(deck.TotalCandy, deck.QuotaCandy);
        HandleRoundStarted(deck.CurrentRound, deck.CurrentRound == deck.TrickRoundIndex);
    }

    void OnDisable()
    {
        if (!deck) return;
        deck.OnEnergyChanged -= HandleEnergyChanged;
        deck.OnCandyChanged -= HandleCandyChanged;
        deck.OnRoundStarted -= HandleRoundStarted;
        deck.OnNightEnded -= HandleNightEnded;
    }

    void HandleEnergyChanged(int current, int max)
    {
        if (energyText) energyText.text = $"Energy: {current}/{max}";
        if (energyPips) energyPips.Set(current, max);
    }

    void HandleCandyChanged(int total, int quota)
    {
        if (candyText) candyText.text = $"Candy: {total}/{quota}";
    }

    void HandleRoundStarted(int roundIndex, bool isTrick)
    {
        if (roundText) roundText.text = $"Round {roundIndex}/{deck.RoundsPerNight}";
        if (modeText) { modeText.text = isTrick ? "TRICK" : "TREAT"; /* color as before */ }
        if (nightText) nightText.text = $"Night {deck.NightIndex}";
    }


    void HandleNightEnded(bool success)
    {
        // Optional: flash a quick message, or leave for your reward screen later.
        // Example: modeText.text = success ? "NIGHT CLEAR!" : "FAILED QUOTA";
    }
}
