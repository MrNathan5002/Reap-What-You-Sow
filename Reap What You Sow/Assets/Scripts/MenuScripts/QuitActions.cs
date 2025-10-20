using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// Optional: support TMP if you're using it
#if TMP_PRESENT || TEXTMESHPRO_PRESENT
using TMPro;
#endif

public class QuitActions : MonoBehaviour
{
    [Header("Scene Names")]
    public string mainMenuScene = "MainMenu";

    [Header("UI")]
    public Button button;
#if TMP_PRESENT || TEXTMESHPRO_PRESENT
    public TextMeshProUGUI tmpLabel;   // assign if using TMP
#endif
    public TMP_Text uiTextLabel;           // assign if using legacy Text

    void Awake()
    {
        if (!button) button = GetComponent<Button>();
        SceneManager.sceneLoaded += OnSceneLoaded;
        Refresh();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene s, LoadSceneMode mode) => Refresh();

    void Refresh()
    {
        if (!button) return;

        bool onMenu = SceneManager.GetActiveScene().name == mainMenuScene;

        // Set label
        SetLabel(onMenu ? "Quit" : "Main Menu");

        // Swap action
        button.onClick.RemoveAllListeners();
        if (onMenu)
            button.onClick.AddListener(QuitGame);
        else
            button.onClick.AddListener(ReturnToMenu);
    }

    void SetLabel(string text)
    {
#if TMP_PRESENT || TEXTMESHPRO_PRESENT
        if (tmpLabel) tmpLabel.text = text;
#endif
        if (uiTextLabel) uiTextLabel.text = text;
    }

    void ReturnToMenu()
    {
        if (!string.IsNullOrEmpty(mainMenuScene))
            SceneManager.LoadScene(mainMenuScene, LoadSceneMode.Single);
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBGL
        // WebGL cannot quit the tab; fallback to going to main menu or do nothing.
        if (!string.IsNullOrEmpty(mainMenuScene))
            SceneManager.LoadScene(mainMenuScene, LoadSceneMode.Single);
#else
        Application.Quit();
#endif
    }
}
