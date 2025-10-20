using UnityEngine;
using UnityEngine.SceneManagement;

public class CutsceneSkipper : MonoBehaviour
{
    [Header("Where to go")]
    public string nextScene = "Main Menu";

    [Header("Optional hooks (use whatever you have)")]
    public TVPowerOffTransition powerOff; // preferred (your TV power-off effect)
    public SceneFader fader;              // fallback fade-to-black
    public SimpleZoomAndLoad zoom;        // cancel ongoing zoom if present

    [Header("Tidy up on skip (optional)")]
    public AudioSource[] fadeOutAudio;    // thunder/BGM/etc. (fast fade)
    public GameObject[] disableObjects;   // e.g., static rotator, lightning, etc.

    [Header("Hotkeys")]
    public KeyCode[] hotkeys = { KeyCode.Escape, KeyCode.Space, KeyCode.Return };

    bool _skipping;

    void Update()
    {
        if (_skipping) return;
        // Hotkey support
        for (int i = 0; i < hotkeys.Length; i++)
            if (Input.GetKeyDown(hotkeys[i])) { Skip(); break; }
    }

    // Wire this to a UI Button's OnClick too
    public void Skip()
    {
        if (_skipping) return;
        _skipping = true;

        // Stop zoom coroutine if running
        if (zoom)
        {
            zoom.StopAllCoroutines();
            zoom.enabled = false;
        }

        // Quickly fade out any scene audio
        if (fadeOutAudio != null)
            foreach (var a in fadeOutAudio)
                if (a) StartCoroutine(FastFadeOut(a, 0.15f));

        // Disable any cutscene-only behaviours/objects
        if (disableObjects != null)
            foreach (var go in disableObjects)
                if (go) go.SetActive(false);

        // Prefer your TV power-off; fallback to SceneFader; final fallback: hard load
        if (powerOff)
        {
            powerOff.PowerOffAndLoad(nextScene);
        }
        else if (fader)
        {
            fader.LoadWithFade(nextScene);
        }
        else
        {
            SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
        }
    }

    System.Collections.IEnumerator FastFadeOut(AudioSource a, float time)
    {
        if (!a) yield break;
        float start = a.volume;
        float t = 0f;
        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            a.volume = Mathf.Lerp(start, 0f, Mathf.Clamp01(t / time));
            yield return null;
        }
        a.volume = 0f;
        a.Stop();
    }
}
