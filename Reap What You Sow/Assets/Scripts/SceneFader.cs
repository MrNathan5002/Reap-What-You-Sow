using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class SceneFader : MonoBehaviour
{
    public CanvasGroup cg;                    // assign the CanvasGroup
    public float fadeInTime = 0.25f;
    public float fadeOutTime = 0.25f;
    public bool useUnscaled = true;

    void Reset() { cg = GetComponentInChildren<CanvasGroup>(); }

    void Awake() { if (!cg) cg = GetComponentInChildren<CanvasGroup>(); }

    public void FadeIn(System.Action onDone = null)  // black -> clear
    { StartCoroutine(CoFade(1f, 0f, fadeInTime, onDone)); }

    public void FadeOut(System.Action onDone = null) // clear -> black
    { StartCoroutine(CoFade(0f, 1f, fadeOutTime, onDone)); }

    public void LoadWithFade(string sceneName)
    {
        FadeOut(() => SceneManager.LoadScene(sceneName, LoadSceneMode.Single));
    }

    IEnumerator CoFade(float from, float to, float dur, System.Action onDone)
    {
        if (!cg) { onDone?.Invoke(); yield break; }
        float t = 0f; cg.alpha = from; cg.blocksRaycasts = true;
        while (t < dur)
        {
            t += useUnscaled ? Time.unscaledDeltaTime : Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / dur));
            yield return null;
        }
        cg.alpha = to; cg.blocksRaycasts = (to > 0.99f);
        onDone?.Invoke();
    }
}
