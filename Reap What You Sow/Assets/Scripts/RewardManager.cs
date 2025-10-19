using System;
using System.Collections.Generic;
using UnityEngine;

public class RewardManager : MonoBehaviour
{
    [Header("Refs")]
    public DeckManager deck;          // assign
    public RewardPanel panel;         // assign

    [Header("Pools")]
    public List<CardPack> packs = new();  // fill with your pack assets

    [Header("Weights (sum doesn't matter)")]
    public int wUpgrade = 50;
    public int wPack = 35;
    public int wPlayer = 10;
    public int wDiscard = 5;

    System.Random rng;

    void Awake()
    {
        if (!deck) deck = FindObjectOfType<DeckManager>();
        if (deck == null) Debug.LogError("[RewardManager] No DeckManager found.");

        rng = new System.Random();
    }

    void OnEnable()
    {
        if (deck != null)
            deck.OnNightEnded += HandleNightEnded;
    }
    void OnDisable()
    {
        if (deck != null)
            deck.OnNightEnded -= HandleNightEnded;
    }

    void HandleNightEnded(bool success)
    {
        if (!success)
        {
            // Simple fail flow: show a single option to restart the run
            if (!panel) { deck.InitializeRun(deck.starter, deck.seed); deck.StartNight(); return; }
            panel.Show("Failed Quota", "Restart Run", "Quit", pick =>
            {
                if (pick == 0)
                {
                    deck.InitializeRun(deck.starter, deck.seed);
                    deck.StartNight();
                }
                else
                {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                }
            });
            return;
        }

        // Success: show 2 choices
        var choices = GenerateTwoChoices();
        if (!panel) { ApplyChoice(choices[0]); deck.StartNight(); return; }

        panel.Show("Night Cleared!",
                   FormatChoice(choices[0]),
                   FormatChoice(choices[1]),
                   pick => {
                       ApplyChoice(choices[Mathf.Clamp(pick, 0, 1)]);
                       deck.StartNight();
                   });
    }

    // ----- Choice generation -----

    public enum RewardType { Upgrade, Pack, PlayerUpgrade, Discard }

    [Serializable]
    public struct RewardChoice
    {
        public RewardType type;
        public CardPack pack;           // for Pack
        public bool playerUpHand;       // for PlayerUpgrade: true=+1 Hand, false=+1 EnergyMax
    }

    RewardChoice[] GenerateTwoChoices()
    {
        var a = RollValidChoice();

        // Reroll B until it's a DIFFERENT TYPE (and valid)
        var b = RollValidChoice();
        int safety = 20;
        while (safety-- > 0 && b.type == a.type)
            b = RollValidChoice();

        return new[] { a, b };
    }

    RewardChoice RollValidChoice()
    {
        for (int attempt = 0; attempt < 10; attempt++)
        {
            var c = RollChoiceOnce();
            if (IsChoiceValid(c)) return c;
        }
        // fallback: player upgrade
        return new RewardChoice { type = RewardType.PlayerUpgrade, playerUpHand = (rng.Next(2) == 0) };
    }

    RewardChoice RollChoiceOnce()
    {
        int total = wUpgrade + wPack + wPlayer + wDiscard;
        int r = rng.Next(1, Math.Max(1, total) + 1);
        if ((r -= wUpgrade) <= 0) return new RewardChoice { type = RewardType.Upgrade };
        if ((r -= wPack) <= 0)
        {
            var pack = (packs != null && packs.Count > 0) ? packs[rng.Next(packs.Count)] : null;
            return new RewardChoice { type = RewardType.Pack, pack = pack };
        }
        if ((r -= wPlayer) <= 0) return new RewardChoice { type = RewardType.PlayerUpgrade, playerUpHand = (rng.Next(2) == 0) };
        return new RewardChoice { type = RewardType.Discard };
    }

    bool IsChoiceValid(RewardChoice c)
    {
        switch (c.type)
        {
            case RewardType.Upgrade:
                return HasUpgradableCopy();
            case RewardType.Pack:
                return (packs != null && packs.Count > 0);
            case RewardType.PlayerUpgrade:
                return true;
            case RewardType.Discard:
                return HasRemovableCard();
        }
        return true;
    }

    string FormatChoice(RewardChoice c)
    {
        switch (c.type)
        {
            case RewardType.Upgrade: return "Upgrade a random card";
            case RewardType.Pack: return c.pack ? $"Pack: {c.pack.packName} (add all)" : "Pack (none available)";
            case RewardType.PlayerUpgrade: return c.playerUpHand ? "+1 Hand Size" : "+1 Max Energy";
            case RewardType.Discard: return "Remove a random card";
            default: return "?";
        }
    }

    // ----- Apply -----

    void ApplyChoice(RewardChoice c)
    {
        switch (c.type)
        {
            case RewardType.Upgrade:
                UpgradeRandomEligible();
                break;
            case RewardType.Pack:
                if (c.pack) c.pack.AddAllToDeck(deck);
                break;
            case RewardType.PlayerUpgrade:
                if (c.playerUpHand) deck.SetHandSize(deck.HandSize + 1);
                else deck.SetEnergyMax(deck.EnergyMax + 1);
                break;
            case RewardType.Discard:
                RemoveRandomCard();
                break;
        }
    }

    // ----- Helpers over DeckManager storage -----

    bool HasUpgradableCopy()
    {
        foreach (var ci in deck.IterAllCopies())
            if (!ci.isUpgraded && ci.def && ci.def.hasUpgrade) return true;
        return false;
    }

    void UpgradeRandomEligible()
    {
        var pool = new List<CardInstance>();
        foreach (var ci in deck.IterAllCopies())
            if (!ci.isUpgraded && ci.def && ci.def.hasUpgrade) pool.Add(ci);
        if (pool.Count == 0) return;
        var pick = pool[rng.Next(pool.Count)];
        pick.isUpgraded = true;
        // Optional: show a toast/log
        Debug.Log($"Upgraded: {(string.IsNullOrEmpty(pick.def.displayName) ? pick.def.name : pick.def.displayName)}");
    }

    bool HasRemovableCard()
    {
        foreach (var ci in deck.IterAllCopies())
            if (ci.def && !ci.def.isSpell) return true;  // avoid removing spells if you want
        return false;
    }

    void RemoveRandomCard()
    {
        var pool = new List<CardInstance>();
        foreach (var ci in deck.IterAllCopies())
            if (ci.def && !ci.def.isSpell) pool.Add(ci);
        if (pool.Count == 0) return;

        var pick = pool[rng.Next(pool.Count)];
        deck.RemoveInstance(pick);
    }
}

