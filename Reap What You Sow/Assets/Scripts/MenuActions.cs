using UnityEngine;

public class MenuActions : MonoBehaviour
{
    public SceneFader fader;
    public string gameSceneName = "Game";

    public void Play()
    {
        // Fade to black, then load Game
        if (fader) fader.LoadWithFade(gameSceneName);
        else UnityEngine.SceneManagement.SceneManager.LoadScene(gameSceneName);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
