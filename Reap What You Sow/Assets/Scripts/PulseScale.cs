using UnityEngine;

[DisallowMultipleComponent]
public class PulseScale : MonoBehaviour
{
    [Header("Pulse Settings")]
    [Tooltip("How many pulses per second.")]
    public float frequency = 1.2f;          // pulses per second
    [Tooltip("How much to grow/shrink (0.1 = ±10%).")]
    [Range(0f, 1f)] public float amplitude = 0.12f; // percent change
    [Tooltip("Optional offset so multiple ghosts aren't in sync.")]
    public float phaseOffset = 0f;          // radians

    [Header("Behavior")]
    public bool useUnscaledTime = true;     // ignore Time.timeScale (menus/pauses)
    public bool pulseUniformly = true;      // uniform XYZ; if false, only XY
    public Vector3 baseScale = Vector3.one; // leave as (1,1,1) to use current

    Vector3 _initialScale;

    void Awake()
    {
        _initialScale = (baseScale == Vector3.one) ? transform.localScale : baseScale;
    }

    void OnEnable()
    {
        // reset so it doesn't drift if re-enabled
        transform.localScale = _initialScale;
    }

    void Update()
    {
        float t = (useUnscaledTime ? Time.unscaledTime : Time.time);
        float s = 1f + amplitude * Mathf.Sin((t * Mathf.PI * 2f * frequency) + phaseOffset);

        if (pulseUniformly)
            transform.localScale = _initialScale * s;
        else
            transform.localScale = new Vector3(_initialScale.x * s, _initialScale.y * s, _initialScale.z);
    }
}
