using System;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    [Header("Config")]
    public StarterDeck starter;                // assign in Inspector
    [Tooltip("Leave -1 for random.")]
    public int seed = -1;

    [Header("Refs")]
    public HandManager hand;                   // assign in Inspector

    [Header("Board")]
    public BoardManager board;

    // Piles
    readonly List<CardInstance> drawPile = new();
    readonly List<CardInstance> discardPile = new();
    readonly List<CardInstance> handLogic = new();

    // Player state
    public int HandSize { get; private set; }
    public int EnergyMax { get; private set; }
    public int Energy { get; private set; }

    // Night/Rounds
    public int RoundsPerNight => starter ? starter.roundsPerNight : 5;
    public int CurrentRound { get; private set; } = 0;
    public int TrickRoundIndex { get; private set; } = 0;
    public int QuotaCandy => starter ? starter.quotaCandy : 40;
    public int TotalCandy { get; private set; } = 0;

    System.Random rng;

    // Events (UI can subscribe)
    public event Action OnDeckChanged;
    public event Action OnHandChanged;
    public event Action<int, int> OnEnergyChanged;     // current, max
    public event Action<int, int> OnCandyChanged;      // total, quota
    public event Action<int, bool> OnRoundStarted;     // roundIndex, isTrick
    public event Action<bool> OnNightEnded;           // success

    void Awake()
    {
        if (!hand) hand = FindObjectOfType<HandManager>();
    }

    void Start()
    {
        InitializeRun(starter, seed);
        if (!board) board = FindObjectOfType<BoardManager>();
        StartNight();
    }

    // --- Run / Night / Round ---

    public void InitializeRun(StarterDeck starterDeck, int seedOverride = -1)
    {
        drawPile.Clear(); discardPile.Clear(); handLogic.Clear();
        TotalCandy = 0; CurrentRound = 0;

        if (!starterDeck)
        {
            Debug.LogError("[DeckManager] No StarterDeck assigned.");
            return;
        }

        HandSize = starterDeck.startHandSize;
        EnergyMax = starterDeck.startEnergyMax;

        int s = (seedOverride == -1) ? UnityEngine.Random.Range(int.MinValue, int.MaxValue) : seedOverride;
        rng = new System.Random(s);

        // Build pile from starter entries
        foreach (var e in starterDeck.entries)
        {
            if (!e.def || e.count <= 0) continue;
            for (int i = 0; i < e.count; i++)
                drawPile.Add(new CardInstance(e.def, e.startUpgraded));
        }

        Shuffle(drawPile);
        OnDeckChanged?.Invoke();
        OnCandyChanged?.Invoke(TotalCandy, QuotaCandy);
    }

    public void StartNight()
    {
        // Pick Trick round
        TrickRoundIndex = rng.Next(1, RoundsPerNight + 1);
        CurrentRound = 0;
        StartNextRound();
    }

    public void StartNextRound()
    {
        CurrentRound++;
        Energy = EnergyMax;
        OnEnergyChanged?.Invoke(Energy, EnergyMax);

        DrawToHandSize();
        OnRoundStarted?.Invoke(CurrentRound, CurrentRound == TrickRoundIndex);
    }

    public void EndTurn()
    {
        // Resolve crops/effects now (Step 2)
        int gained = 0;
        if (board)
        {
            bool isTrick = (CurrentRound == TrickRoundIndex);
            gained = board.ResolveRound(includeDiagonals: true, isTrickRound: isTrick);
        }
        ResolveRoundCandy(gained);

        DiscardHandAll();
        if (CurrentRound < RoundsPerNight)
        {
            StartNextRound();
        }
        else
        {
            bool success = TotalCandy >= QuotaCandy;
            OnNightEnded?.Invoke(success);
            // Rewards later
        }
    }


    void ResolveRoundCandy(int gained)
    {
        TotalCandy += Mathf.Max(0, gained);
        OnCandyChanged?.Invoke(TotalCandy, QuotaCandy);
    }

    // --- Drawing / Discard ---

    public void DrawToHandSize()
    {
        if (!hand) { Debug.LogWarning("[DeckManager] No HandManager assigned."); return; }

        while (handLogic.Count < HandSize)
        {
            if (drawPile.Count == 0)
            {
                if (discardPile.Count == 0) break;
                // reshuffle discard into draw
                drawPile.AddRange(discardPile);
                discardPile.Clear();
                Shuffle(drawPile);
            }

            var top = PopTop(drawPile);
            handLogic.Add(top);
            hand.AddCardToHand(top.def); // visuals still take CardEditor for now
        }
        OnHandChanged?.Invoke();
        OnDeckChanged?.Invoke();
    }

    public void SpendEnergy(int amount)
    {
        Energy = Mathf.Max(0, Energy - Mathf.Max(0, amount));
        OnEnergyChanged?.Invoke(Energy, EnergyMax);
    }

    public bool CanAfford(int cost) => Energy >= cost;

    public void DiscardFromHand(CardInstance ci)
    {
        if (handLogic.Remove(ci))
        {
            discardPile.Add(ci);
            OnHandChanged?.Invoke();
            OnDeckChanged?.Invoke();
        }
    }

    public void DiscardHandAll()
    {
        // Kill visuals and clear list
        if (hand) hand.DiscardAll(); // add this helper to HandManager (below)

        discardPile.AddRange(handLogic);
        handLogic.Clear();

        OnHandChanged?.Invoke();
        OnDeckChanged?.Invoke();
    }

    // --- Mutations used later by rewards/shop ---

    public void AddCard(CardEditor def, int copies = 1, bool upgraded = false)
    {
        for (int i = 0; i < copies; i++)
            discardPile.Add(new CardInstance(def, upgraded));
        OnDeckChanged?.Invoke();
    }

    public bool RemoveOneByDef(CardEditor def)
    {
        // Prefer removing from discard, then draw; avoid touching hand here
        int idx = discardPile.FindIndex(c => c.def == def);
        if (idx >= 0) { discardPile.RemoveAt(idx); OnDeckChanged?.Invoke(); return true; }
        idx = drawPile.FindIndex(c => c.def == def);
        if (idx >= 0) { drawPile.RemoveAt(idx); OnDeckChanged?.Invoke(); return true; }
        return false;
    }

    // --- Helpers ---

    T PopTop<T>(List<T> list)
    {
        int last = list.Count - 1;
        var x = list[last];
        list.RemoveAt(last);
        return x;
    }

    void Shuffle<T>(List<T> list)
    {
        // Fisher–Yates using our run RNG
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // Queries for a simple deck tracker UI (hover tooltips)
    public Dictionary<CardEditor, int> GetCountsInDraw()
    {
        var d = new Dictionary<CardEditor, int>();
        foreach (var c in drawPile) d[c.def] = d.GetValueOrDefault(c.def) + 1;
        return d;
    }
    public Dictionary<CardEditor, int> GetCountsInDiscard()
    {
        var d = new Dictionary<CardEditor, int>();
        foreach (var c in discardPile) d[c.def] = d.GetValueOrDefault(c.def) + 1;
        return d;
    }

    public bool RemoveOneByDefFromHand(CardEditor def)
    {
        int idx = handLogic.FindIndex(ci => ci.def == def);
        if (idx >= 0)
        {
            var ci = handLogic[idx];
            handLogic.RemoveAt(idx);
            discardPile.Add(ci);
            OnHandChanged?.Invoke();
            OnDeckChanged?.Invoke();
            return true;
        }
        return false;
    }

}
