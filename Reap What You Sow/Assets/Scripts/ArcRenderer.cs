using System.Collections.Generic;
using UnityEngine;

public class ArcRenderer : MonoBehaviour
{
    public GameObject arrowPrefab;
    public GameObject dotPrefab;
    public int poolSize = 50;

    private readonly List<GameObject> dotPool = new List<GameObject>();
    private GameObject arrowInstance;

    [Header("Tuning (world units)")]
    [Tooltip("Distance between dots in WORLD units (e.g., 0.15f ~ 2.4px at 16 PPU).")]
    public float spacing = 0.2f; // was 50 (way too large in world units)

    public float arrowAngleAdjustment = 0f;
    [Tooltip("Reserve this many dots near the cursor for the arrow (usually 1).")]
    public int dotsToSkip = 1;

    private Vector3 arrowTailWorldPos; // prev dot before arrow

    void Start()
    {
        arrowInstance = Instantiate(arrowPrefab, transform);
        arrowInstance.transform.localPosition = Vector3.zero;
        InitializeDotPool(poolSize);
    }

    void Update()
    {
        Vector3 mousePos = GetMouseWorldPosition();
        Vector3 startPos = transform.position;
        Vector3 midPoint = CalculateMidPoint(startPos, mousePos);

        UpdateArc(startPos, midPoint, mousePos);
        PositionAndRotateArrow(mousePos);
    }

    Vector3 GetMouseWorldPosition()
    {
        // Works with both old and new input systems:
        Vector3 screen = Input.mousePosition; // fallback
#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Mouse.current != null)
            screen = (Vector3)UnityEngine.InputSystem.Mouse.current.position.ReadValue();
#endif
        // For orthographic cams, Z is ignored; for perspective we need distance to the plane of the arc.
        var cam = Camera.main;
        float z = Mathf.Abs(transform.position.z - cam.transform.position.z);
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, z));
        world.z = transform.position.z; // keep on our plane
        return world;
    }

    void UpdateArc(Vector3 start, Vector3 mid, Vector3 end)
    {
        float dist = Vector3.Distance(start, end);
        int numDots = Mathf.Clamp(Mathf.CeilToInt(dist / Mathf.Max(0.0001f, spacing)), 1, dotPool.Count);

        // Default arrow tail to start in case we don't set it below
        arrowTailWorldPos = start;

        for (int i = 0; i < dotPool.Count; i++) dotPool[i].SetActive(false);

        for (int i = 0; i < numDots; i++)
        {
            float t = Mathf.Clamp01(i / (float)numDots);
            Vector3 position = QuadraticBezierPoint(start, mid, end, t);

            // Leave the last 'dotsToSkip' indices for the arrow tip space
            if (i < numDots - dotsToSkip)
            {
                dotPool[i].transform.position = position;
                dotPool[i].SetActive(true);
                arrowTailWorldPos = position; // last active dot becomes arrow tail
            }
        }
    }

    void PositionAndRotateArrow(Vector3 arrowTipWorldPos)
    {
        arrowInstance.transform.position = arrowTipWorldPos;

        // Point FROM the last dot TO the cursor (tip)
        Vector3 dir = arrowTipWorldPos - arrowTailWorldPos;
        if (dir.sqrMagnitude < 1e-6f) dir = Vector3.right; // safe default

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + arrowAngleAdjustment;
        arrowInstance.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    Vector3 CalculateMidPoint(Vector3 start, Vector3 end)
    {
        Vector3 midpoint = (start + end) * 0.5f;
        float arcHeight = Vector3.Distance(start, end) / 3f;
        midpoint.y += arcHeight;
        return midpoint;
    }

    Vector3 QuadraticBezierPoint(Vector3 start, Vector3 control, Vector3 end, float t)
    {
        float u = 1f - t;
        return u * u * start + 2f * u * t * control + t * t * end;
    }

    void InitializeDotPool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject dot = Instantiate(dotPrefab, Vector3.zero, Quaternion.identity, transform);
            dot.SetActive(false);
            dotPool.Add(dot);
        }
    }
}
