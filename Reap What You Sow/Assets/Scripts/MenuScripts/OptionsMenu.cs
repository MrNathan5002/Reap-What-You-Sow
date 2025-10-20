// OptionsMenuSimple.cs  (add the static bits + methods)
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    public static OptionsMenu Instance;   // ← add

    [Header("UI")]
    public GameObject panel;
    public Slider volumeSlider;

    [Header("Settings")]
    [Range(0f, 1f)] public float defaultVolume = 0.8f;
    public string prefsKey = "master_volume";

    void Awake()
    {
        // make globally accessible (pairs nicely with your PersistAcrossScenes)
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (panel) panel.SetActive(false);

        float v = PlayerPrefs.GetFloat(prefsKey, defaultVolume);
        ApplyVolume(v);

        if (volumeSlider)
        {
            volumeSlider.minValue = 0f; volumeSlider.maxValue = 1f;
            volumeSlider.value = v;
            volumeSlider.onValueChanged.AddListener(OnSliderChanged);
        }
    }

    // --- Static shortcuts for UI buttons ---
    public static void ToggleGlobal() => Instance?.TogglePanel();
    public static void OpenGlobal() { if (Instance && Instance.panel) Instance.panel.SetActive(true); }
    public static void CloseGlobal() { if (Instance && Instance.panel) Instance.panel.SetActive(false); }

    // --- Instance methods you already had ---
    public void TogglePanel()
    {
        if (!panel) return;
        panel.SetActive(!panel.activeSelf);
    }

    void OnSliderChanged(float v)
    {
        ApplyVolume(v);
        PlayerPrefs.SetFloat(prefsKey, v);
        PlayerPrefs.Save();
    }

    void ApplyVolume(float v) => AudioListener.volume = Mathf.Clamp01(v);
}
