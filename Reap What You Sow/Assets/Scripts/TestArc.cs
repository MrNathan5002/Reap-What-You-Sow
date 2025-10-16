using System.Collections.Generic;
using UnityEngine;

public class TestArc : MonoBehaviour
{
    [Header("Prefabs & Pool")]
    public GameObject arrowPrefab;
    public GameObject dotPrefab;
    public int poolSize = 50;

    [Header("Arc Settings")]
    [Tooltip("Approximate world-space spacing between dots. Make small (0.1 - 1) for visible dots")]
    public float spacing = 0.5f;
    public float arrowAngleAdjustment = 0f;
    public int dotsToSkip = 1;

    private List<GameObject> dotPool = new List<GameObject>();
    private GameObject arrowInstance;
    private Vector3 arrowDirection;
    private Camera mainCam;

    void Awake()
    {
        Debug.Log("[ArcRenderer] Awake");
        // cache camera and validate
        mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogWarning("[ArcRenderer] No Camera tagged 'MainCamera' found. Attempting to use Camera.main at Awake returned null.");
        }

        // defensive: clamp values
        if (poolSize < 1) poolSize = 1;
        if (spacing <= 0f) spacing = 0.5f;
        if (dotsToSkip < 0) dotsToSkip = 0;
    }

    void Start()
    {
        Debug.Log("[ArcRenderer] Start - initializing pool and arrow");
        InitializeDotPool(poolSize);

        if (arrowPrefab != null)
        {
            arrowInstance = Instantiate(arrowPrefab, transform);
            arrowInstance.transform.localPosition = Vector3.zero;
            arrowInstance.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[ArcRenderer] arrowPrefab not assigned in inspector. Arrow will be disabled.");
        }

        if (dotPrefab == null)
        {
            Debug.LogWarning("[ArcRenderer] dotPrefab not assigned in inspector. Dots will not be visible.");
        }
    }

    void Update()
    {
        // if script/component disabled this won't run; log to confirm Update is running
        // but don't spam: only log first frame
        if (Time.frameCount == 1)
            Debug.Log("[ArcRenderer] Update running");

        // ensure camera available
        if (mainCam == null)
        {
            mainCam = Camera.main;
            if (mainCam == null)
            {
                // Can't convert mouse to world without a camera
                Debug.LogError("[ArcRenderer] No Camera.main available. Ensure your camera has the tag 'MainCamera'.");
                return;
            }
        }

        Camera cam = Camera.main;
        float zDist = Mathf.Abs(cam.transform.position.z - transform.position.z);
        Vector3 mousePos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, zDist));
        mousePos.z = transform.position.z;

        Vector3 startPos = transform.position;
        Vector3 midPoint = CalculateMidPoint(startPos, mousePos);

        UpdateArc(startPos, midPoint, mousePos);
        PositionAndRotateArrow(mousePos);
    }

    void UpdateArc(Vector3 start, Vector3 mid, Vector3 end)
    {
        float distance = Vector3.Distance(start, end);
        int numDots = Mathf.CeilToInt(distance / spacing);

        if (numDots <= 0)
        {
            Debug.Log("[ArcRenderer] numDots <= 0, distance: " + distance + " spacing: " + spacing);
            // hide entire pool
            for (int j = 0; j < dotPool.Count; j++) dotPool[j].SetActive(false);
            return;
        }

        // clamp numDots so we don't exceed pool
        int usableDots = Mathf.Min(numDots, dotPool.Count);

        // Debug log once per update (if you get too many logs comment this line)
        Debug.Log("[ArcRenderer] distance=" + distance + " spacing=" + spacing + " numDots=" + numDots + " usableDots=" + usableDots);

        for (int i = 0; i < usableDots; i++)
        {
            float t = i / (float)numDots; // note: using numDots for parameterization keeps arcs consistent
            t = Mathf.Clamp01(t);

            Vector3 position = QuadraticBezierPoint(start, mid, end, t);

            GameObject dot = dotPool[i];
            if (dot != null)
            {
                dot.transform.position = position;
                if (!dot.activeSelf) dot.SetActive(true);
            }

            // identify arrowDirection from the last visible dot (before skipped dots)
            if (i == usableDots - (dotsToSkip + 1))
            {
                arrowDirection = position;
            }
        }

        // deactivate any remaining dots in pool
        for (int i = usableDots; i < dotPool.Count; i++)
        {
            if (dotPool[i] != null && dotPool[i].activeSelf)
                dotPool[i].SetActive(false);
        }
    }

    void PositionAndRotateArrow(Vector3 position)
    {
        if (arrowInstance == null) return;

        // Place arrow at the provided position (end). You can change this to place it on the curve.
        arrowInstance.transform.position = position;

        // compute direction from arrowDirection to the arrow position (so arrow points along curve)
        Vector3 direction = position - arrowDirection;
        if (direction.sqrMagnitude <= 0.0001f)
        {
            // fallback: point right (no rotation changes)
            return;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle += arrowAngleAdjustment;
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
        float tt = t * t;
        float uu = u * u;
        return uu * start + 2f * u * t * control + tt * end;
    }

    void InitializeDotPool(int count)
    {
        Debug.Log("[ArcRenderer] InitializeDotPool count = " + count);
        dotPool.Clear();

        if (dotPrefab == null)
        {
            Debug.LogWarning("[ArcRenderer] dotPrefab is null: pool will be filled with null placeholders (no dots visible). Assign a prefab to see dots.");
            // create null placeholders to avoid index issues
            for (int i = 0; i < count; i++) dotPool.Add(null);
            return;
        }

        for (int i = 0; i < count; i++)
        {
            GameObject dot = Instantiate(dotPrefab, transform);
            dot.transform.localPosition = Vector3.zero;
            dot.SetActive(false);
            dotPool.Add(dot);
        }
    }
}