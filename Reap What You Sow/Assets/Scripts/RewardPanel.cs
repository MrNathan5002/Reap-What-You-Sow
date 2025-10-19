using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardPanel : MonoBehaviour
{
    [Header("UI")]
    public GameObject panel;      // parent panel GameObject
    public TMP_Text titleText;        // e.g., "Night Cleared!"
    public Button optionAButton;
    public Button optionBButton;
    public TMP_Text optionAText;
    public TMP_Text optionBText;

    System.Action<int> _onPick;

    void Awake()
    {
        if (panel) panel.SetActive(false);
        optionAButton.onClick.AddListener(() => Pick(0));
        optionBButton.onClick.AddListener(() => Pick(1));
    }

    public void Show(string title, string aLabel, string bLabel, System.Action<int> onPick)
    {
        if (titleText) titleText.text = title;
        if (optionAText) optionAText.text = aLabel;
        if (optionBText) optionBText.text = bLabel;
        _onPick = onPick;
        if (panel) panel.SetActive(true);
    }

    public void Hide()
    {
        if (panel) panel.SetActive(false);
        _onPick = null;
    }

    void Pick(int i)
    {
        var cb = _onPick;
        Hide();
        cb?.Invoke(i);
    }
}

