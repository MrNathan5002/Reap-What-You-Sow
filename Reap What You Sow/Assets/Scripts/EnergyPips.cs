using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnergyPips : MonoBehaviour
{
    [Header("Pip Prefab (UI Image)")]
    public Image pipPrefab;
    public Color filled = Color.white;
    public Color empty = new Color(1f, 1f, 1f, 0.25f);

    readonly List<Image> _pips = new();

    public void Set(int current, int max)
    {
        if (!pipPrefab) return;

        // Ensure pool size
        while (_pips.Count < max)
        {
            var img = Instantiate(pipPrefab, transform);
            _pips.Add(img);
        }
        // Hide extras if max decreased
        for (int i = 0; i < _pips.Count; i++)
            _pips[i].gameObject.SetActive(i < max);

        // Update colors
        for (int i = 0; i < max; i++)
            _pips[i].color = (i < current) ? filled : empty;
    }
}
