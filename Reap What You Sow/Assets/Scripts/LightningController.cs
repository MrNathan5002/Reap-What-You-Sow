using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal; // Light2D

public class LightningFlash2D : MonoBehaviour
{
    public enum TimeSource { GameTime, UnscaledTime, AudioTime }

    [Header("Schedule (seconds)")]
    public List<float> flashTimes = new List<float>();   // e.g. 0.8, 1.12, 3.2

    [Header("Clock")]
    public TimeSource timeSource = TimeSource.GameTime;
    public AudioSource audioSource; // only used if TimeSource = AudioTime
    public bool useUnscaledForDurations = true;

    [Header("Targets")]
    public Light2D lightningLight;    // your URP 2D light
    public Camera targetCamera;       // usually your menu camera

    [Header("Flash Look")]
    public Color flashColor = Color.white;
    [Tooltip("How long the light & white background stay at peak.")]
    public float hold = 0.06f;
    [Tooltip("How long to fade the camera background back to normal.")]
    public float fadeBack = 0.12f;

    [Header("Misc")]
    public bool ensureLightObjectActive = true; // enable gameObject if it’s disabled

    // state
    int _nextIdx;
    Color _baseCamColor;

    void Awake()
    {
        if (!targetCamera) targetCamera = Camera.main;
        _baseCamColor = targetCamera ? targetCamera.backgroundColor : Color.black;

        flashTimes.Sort();
        _nextIdx = 0;

        if (lightningLight)
        {
            if (ensureLightObjectActive && !lightningLight.gameObject.activeSelf)
                lightningLight.gameObject.SetActive(true);
            lightningLight.enabled = false; // start off, as requested
        }
    }

    void Update()
    {
        if (_nextIdx >= flashTimes.Count) return;

        float now = CurrentTime();
        while (_nextIdx < flashTimes.Count && now >= flashTimes[_nextIdx])
        {
            StartCoroutine(FlashOnce());
            _nextIdx++;
        }
    }

    float CurrentTime()
    {
        switch (timeSource)
        {
            case TimeSource.AudioTime:
                return (audioSource && audioSource.clip) ? audioSource.time : 0f;
            case TimeSource.UnscaledTime:
                return Time.unscaledTime;
            default:
                return Time.time;
        }
    }

    System.Collections.IEnumerator FlashOnce()
    {
        // Peak on: camera to white + light on
        if (targetCamera) targetCamera.backgroundColor = flashColor;
        if (lightningLight) lightningLight.enabled = true;

        // Hold
        float left = hold;
        while (left > 0f)
        {
            left -= useUnscaledForDurations ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }

        // Light off immediately
        if (lightningLight) lightningLight.enabled = false;

        // Fade camera color back
        if (targetCamera && fadeBack > 0f)
        {
            float t = 0f;
            Color start = targetCamera.backgroundColor; // should be flashColor
            while (t < fadeBack)
            {
                t += useUnscaledForDurations ? Time.unscaledDeltaTime : Time.deltaTime;
                float u = Mathf.Clamp01(t / fadeBack);
                targetCamera.backgroundColor = Color.Lerp(start, _baseCamColor, u);
                yield return null;
            }
        }

        // Final snap to base
        if (targetCamera) targetCamera.backgroundColor = _baseCamColor;
    }

    // Handy buttons
    [ContextMenu("Test One Flash")]
    void TestFlash() { StartCoroutine(FlashOnce()); }

    [ContextMenu("Reset Schedule")]
    public void ResetSchedule()
    {
        _nextIdx = 0;
        if (targetCamera) targetCamera.backgroundColor = _baseCamColor;
        if (lightningLight) lightningLight.enabled = false;
    }
}
