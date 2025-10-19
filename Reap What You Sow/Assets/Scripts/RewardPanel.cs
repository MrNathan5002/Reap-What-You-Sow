using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardPanel : MonoBehaviour
{
    [Header("UI")]
    public GameObject panel;      // container child you toggle on/off
    public TMP_Text titleText;
    public Button optionAButton;
    public Button optionBButton;
    public TMP_Text optionAText;
    public TMP_Text optionBText;
    public CanvasGroup cg;        // OPTIONAL: assign on the same root GO (recommended)

    System.Action<int> _onPick;
    bool _wired;

    void Awake()
    {
        // If you forgot to drag a CanvasGroup, try to find one (optional)
        if (!cg) cg = GetComponent<CanvasGroup>();
        // Start hidden but keep root active if it's already active
        if (panel) panel.SetActive(false);
        if (cg) { cg.alpha = 0f; cg.interactable = false; cg.blocksRaycasts = false; }
    }

    void WireButtons()
    {
        if (_wired) return;
        if (optionAButton) optionAButton.onClick.AddListener(() => Pick(0));
        if (optionBButton) optionBButton.onClick.AddListener(() => Pick(1));
        _wired = true;
    }

    public void Show(string title, string aLabel, string bLabel, System.Action<int> onPick)
    {
        // <<< Ensure root is active (Option B)
        if (!gameObject.activeSelf) gameObject.SetActive(true);

        // Always wire on show (safe if already wired)
        WireButtons();

        // Fill labels
        if (titleText) titleText.text = title;
        if (optionAText) optionAText.text = aLabel;
        if (optionBText) optionBText.text = bLabel;
        _onPick = onPick;

        // Make sure panel is visible and clickable
        if (panel) panel.SetActive(true);
        if (cg) { cg.alpha = 1f; cg.interactable = true; cg.blocksRaycasts = true; }
    }

    public void Hide()
    {
        if (cg) { cg.alpha = 0f; cg.interactable = false; cg.blocksRaycasts = false; }
        if (panel) panel.SetActive(false);
        // keep root active so next Show works reliably
        _onPick = null;
    }

    void Pick(int i)
    {
        var cb = _onPick;
        Hide();
        cb?.Invoke(i);
    }
}
