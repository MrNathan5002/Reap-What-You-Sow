using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class PileHoverTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public enum Pile { Draw, Discard }

    [Header("Config")]
    public Pile pile = Pile.Draw;
    public GameObject tooltipPanel;      // enable/disable this on hover
    public TMP_Text tooltipText;             // where we write counts

    [Header("Refs")]
    public DeckManager deck;

    void Awake()
    {
        if (!deck) deck = FindObjectOfType<DeckManager>();
        if (tooltipPanel) tooltipPanel.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData e)
    {
        if (!tooltipPanel || !tooltipText || deck == null) return;

        Dictionary<CardEditor, int> counts =
            (pile == Pile.Draw) ? deck.GetCountsInDraw() : deck.GetCountsInDiscard();

        var sb = new StringBuilder();
        foreach (var kv in Sorted(counts))
        {
            var def = kv.Key;
            int count = kv.Value;
            if (!def) continue;
            string name = string.IsNullOrEmpty(def.displayName) ? def.name : def.displayName;
            sb.AppendLine($"{name} × {count}");
        }

        tooltipText.text = sb.Length > 0 ? sb.ToString() : "(Empty)";
        tooltipPanel.SetActive(true);
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (tooltipPanel) tooltipPanel.SetActive(false);
    }

    // simple alphabetical sort by display name
    static List<KeyValuePair<CardEditor, int>> Sorted(Dictionary<CardEditor, int> map)
    {
        var list = new List<KeyValuePair<CardEditor, int>>(map);
        list.Sort((a, b) => string.Compare(
            string.IsNullOrEmpty(a.Key?.displayName) ? a.Key?.name : a.Key.displayName,
            string.IsNullOrEmpty(b.Key?.displayName) ? b.Key?.name : b.Key.displayName,
            System.StringComparison.OrdinalIgnoreCase));
        return list;
    }
}
