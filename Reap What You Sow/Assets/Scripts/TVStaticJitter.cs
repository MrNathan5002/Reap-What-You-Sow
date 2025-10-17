using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class TVStaticJitter : MonoBehaviour
{
    [Header("Update cadence")]
    [Tooltip("How many jumps per second.")]
    public float updatesPerSecond = 12f;
    [Tooltip("Randomize the interval by this ± fraction (0.25 = ±25%).")]
    [Range(0f, 1f)] public float intervalJitter = 0.25f;
    public bool useUnscaledTime = true;

    [Header("Angles")]
    [Tooltip("Use this fixed list instead of a range (e.g., -8, -4, 0, 4, 8).")]
    public List<float> discreteAngles = new List<float>();
    [Tooltip("If not using discrete angles, pick in this range (degrees).")]
    public float minAngle = -10f, maxAngle = 10f;
    [Tooltip("Snap random angle to step size (e.g., 1°, 2°, 5°). 0 = off.")]
    public float angleStep = 1f;

    float _timer;
    float _interval;

    void OnEnable()
    {
        ScheduleNext();
        Jitter();
    }

    void Update()
    {
        _timer += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        if (_timer >= _interval)
        {
            _timer = 0f;
            ScheduleNext();
            Jitter();
        }
    }

    void ScheduleNext()
    {
        float baseInterval = (updatesPerSecond <= 0f) ? 0.1f : 1f / updatesPerSecond;
        float j = 1f + Random.Range(-intervalJitter, intervalJitter);
        _interval = Mathf.Max(0.001f, baseInterval * j);
    }

    void Jitter()
    {
        float angle;
        if (discreteAngles != null && discreteAngles.Count > 0)
        {
            angle = discreteAngles[Random.Range(0, discreteAngles.Count)];
        }
        else
        {
            angle = Random.Range(minAngle, maxAngle);
            if (angleStep > 0f) angle = Mathf.Round(angle / angleStep) * angleStep;
        }
        transform.localRotation = Quaternion.Euler(0f, 0f, angle);
    }
}
