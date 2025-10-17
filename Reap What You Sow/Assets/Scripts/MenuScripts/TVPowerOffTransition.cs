using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal; // Light2D

[DisallowMultipleComponent]
public class TVPowerOffTransition : MonoBehaviour
{
    [Header("Targets")]
    [Tooltip("The transform that holds ONLY the TV screen sprite (scale will be animated).")]
    public Transform screen;                     // required
    [Tooltip("Optional SpriteRenderer for color flash (auto-fills from 'screen' if omitted).")]
    public SpriteRenderer screenRenderer;        // optional
    [Tooltip("The TV's 2D light (glow) to fade out.")]
    public Light2D tvLight;                      // optional

    [Header("Timings (seconds)")]
    public float flashBrightTime = 0.06f;        // quick white pop
    public float collapseTime = 0.22f;           // collapse to a line
    public float lineHoldTime = 0.05f;           // pause on the thin line
    public float lightFadeTime = 0.15f;          // TV glow fade
    public bool useUnscaledTime = true;

    [Header("Look")]
    public Color flashColor = Color.white;       // momentary flash tint
    [Tooltip("How thin the final horizontal line becomes (local Y scale).")]
    public float lineThickness = 0.02f;          // in local scale units
    [Tooltip("Slight horizontal pinch at the very end for CRT feel.")]
    public float endXSquash = 0.85f;             // 1 = none, <1 = pinch
    public AnimationCurve collapseEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Audio (optional)")]
    public AudioSource oneShotSource;
    public AudioClip powerPop;                   // little 'pop' when it shuts off
    [Range(0f, 1f)] public float popVolume = 0.5f;

    // stash
    Vector3 _startScale;
    Color _startColor;
    float _startLightIntensity;

    void Awake()
    {
        if (!screen)
        {
            Debug.LogError("[TVPowerOffTransition] Assign 'screen' (the screen's Transform).");
            enabled = false; return;
        }
        if (!screenRenderer) screen.TryGetComponent(out screenRenderer);
        _startScale = screen.localScale;
        if (screenRenderer) _startColor = screenRenderer.color;
        if (tvLight) _startLightIntensity = tvLight.intensity;
    }

    /// <summary>Run the TV power-off, then load the scene (single load).</summary>
    public void PowerOffAndLoad(string sceneName)
    {
        StartCoroutine(CoPowerOff(() =>
        {
            if (!string.IsNullOrEmpty(sceneName))
                SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }));
    }

    /// <summary>Run the TV power-off, then call your callback (for custom handoffs).</summary>
    public void PowerOff(System.Action onComplete = null)
    {
        StartCoroutine(CoPowerOff(onComplete));
    }

    IEnumerator CoPowerOff(System.Action onDone)
    {
        // 1) quick white flash + optional pop
        if (oneShotSource && powerPop) oneShotSource.PlayOneShot(powerPop, popVolume);
        if (screenRenderer) screenRenderer.color = flashColor;

        // kick light to peak briefly (optional)
        if (tvLight) tvLight.enabled = true;

        float t = 0f;
        while (t < flashBrightTime)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }

        // 2) collapse: Y -> lineThickness, X -> endXSquash * startX
        Vector3 from = _startScale;
        Vector3 to = new Vector3(_startScale.x * endXSquash, lineThickness, _startScale.z);

        t = 0f;
        while (t < collapseTime)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float k = collapseEase.Evaluate(Mathf.Clamp01(t / Mathf.Max(0.0001f, collapseTime)));
            // ease Y a bit faster for that snappy feel
            float ky = Mathf.SmoothStep(0, 1, k);
            float kx = k * 0.8f + 0.2f * k; // subtle timing bias

            float newX = Mathf.Lerp(from.x, to.x, kx);
            float newY = Mathf.Lerp(from.y, to.y, ky);
            screen.localScale = new Vector3(newX, newY, from.z);

            // fade the light while collapsing
            if (tvLight && lightFadeTime > 0f)
            {
                float lf = Mathf.Clamp01(t / lightFadeTime);
                tvLight.intensity = Mathf.Lerp(_startLightIntensity, 0f, lf);
            }

            // tint back toward original as it collapses
            if (screenRenderer)
            {
                screenRenderer.color = Color.Lerp(flashColor, _startColor, k);
            }

            yield return null;
        }
        screen.localScale = to;

        // 3) hold the white line for a beat
        t = 0f;
        while (t < lineHoldTime)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }

        // 4) ensure light off and reset color (we’ll restore scale/color after scene change)
        if (tvLight) { tvLight.intensity = 0f; tvLight.enabled = false; }
        if (screenRenderer) screenRenderer.color = _startColor;

        // done → call back (e.g., load scene / start fade)
        onDone?.Invoke();
    }

    // Optional helper to restore screen state (useful if you stay in-scene)
    public void Restore()
    {
        screen.localScale = _startScale;
        if (screenRenderer) screenRenderer.color = _startColor;
        if (tvLight) { tvLight.intensity = _startLightIntensity; tvLight.enabled = true; }
    }
}