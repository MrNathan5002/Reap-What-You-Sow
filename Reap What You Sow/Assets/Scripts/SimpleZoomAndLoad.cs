using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class SimpleZoomAndLoad : MonoBehaviour
{
    [Header("Target & Zoom")]
    public Transform target;              // TV screen center
    public float duration = 1.0f;         // zoom time
    public float finalOrthoSize = 2.5f;   // orthographic cams
    public float finalFOV = 25f;          // perspective cams
    public bool lockZ = true;             // keep initial Z in 2D

    [Header("Timing")]
    public float startDelay = 0f;         // delay before zoom
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool useUnscaledTime = true;

    [Header("Pixel Art (optional)")]
    public int pixelsPerUnit = 0;         // e.g., 16 to snap during move; 0 = off

    [Header("Transition")]
    public string gameSceneName = "MainMenu";
    [Tooltip("TV power-off effect. If missing, will load the scene directly after zoom.")]
    public TVPowerOffTransition tvPowerOff;   // assign in Inspector (recommended)

    Camera _cam;
    bool _busy;

    void Awake()
    {
        _cam = GetComponent<Camera>();
        if (!tvPowerOff) tvPowerOff = FindObjectOfType<TVPowerOffTransition>(true);
    }

    IEnumerator Start()
    {
        if (_busy) yield break;
        _busy = true;

        if (!target) { Debug.LogWarning("[SimpleZoomAndLoad] No target set."); yield break; }

        // Optional delay before zoom
        if (startDelay > 0f)
        {
            if (useUnscaledTime) yield return new WaitForSecondsRealtime(startDelay);
            else yield return new WaitForSeconds(startDelay);
        }

        // Do the zoom
        yield return StartCoroutine(CoZoom());

        // Now trigger the TV power-off OR load directly
        if (tvPowerOff)
        {
            tvPowerOff.PowerOffAndLoad(gameSceneName);
        }
        else
        {
            SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
        }

        _busy = false;
    }

    IEnumerator CoZoom()
    {
        Vector3 startPos = transform.position;
        float startZoom = _cam.orthographic ? _cam.orthographicSize : _cam.fieldOfView;

        Vector3 endPos = target.position;
        if (lockZ) endPos.z = startPos.z;

        float t = 0f;
        while (t < duration)
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            t += dt;
            float u = Mathf.Clamp01(t / duration);
            float k = ease.Evaluate(u);

            // Position
            Vector3 pos = Vector3.Lerp(startPos, endPos, k);
            if (pixelsPerUnit > 0)
            {
                float pix = 1f / pixelsPerUnit;
                pos.x = Mathf.Round(pos.x / pix) * pix;
                pos.y = Mathf.Round(pos.y / pix) * pix;
            }
            transform.position = pos;

            // Zoom
            if (_cam.orthographic)
                _cam.orthographicSize = Mathf.Lerp(startZoom, finalOrthoSize, k);
            else
                _cam.fieldOfView = Mathf.Lerp(startZoom, finalFOV, k);

            yield return null;
        }

        // Final snap
        if (pixelsPerUnit > 0)
        {
            float pix = 1f / pixelsPerUnit;
            var pos = transform.position;
            pos.x = Mathf.Round(pos.x / pix) * pix;
            pos.y = Mathf.Round(pos.y / pix) * pix;
            transform.position = pos;
        }
        if (_cam.orthographic) _cam.orthographicSize = finalOrthoSize;
        else _cam.fieldOfView = finalFOV;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!target) return;
        Gizmos.color = new Color(0.2f, 0.9f, 1f, 0.8f);
        Gizmos.DrawWireSphere(target.position, 0.1f);
    }
#endif
}
