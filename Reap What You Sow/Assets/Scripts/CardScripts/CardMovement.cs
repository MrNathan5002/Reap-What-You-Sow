using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CardMovement : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IEndDragHandler
{
    private RectTransform rectTransform;      // present if UI
    private Canvas canvas;                    // present if UI
    private bool isUI;                        // UI mode vs world-sprite mode

    private Vector3 originalWorldPointer;     // world drag start (not used to move now)
    private Vector3 originalWorldPos;         // world start pos (for restore)

    private Vector3 originalScale;
    private int currentState = 0;             // 0 idle, 1 hover, 2 dragging
    private Quaternion originalRotation;
    private Vector3 originalLocalPos;
    private InputAction mousePointer;
    private Camera cam;

    [Header("Hover Look")]
    [SerializeField] private float selectScale = 1.1f;
    [SerializeField] private float hoverYOffset = 2f;
    [SerializeField] private float hoverZOffset = -0.5f; // negative pulls toward cam

    [Header("Visuals")]
    [SerializeField] private GameObject glowEffect;

    [Header("Sorting Boost (world sprites)")]
    [SerializeField] private int sortingOrderBoost = 100;

    private ArcRenderer arc;                  // arc on this card only
    private bool elevated;                    // are we currently elevated?
    private int originalSiblingIndex = -1;    // UI ordering
    private int originalSortingOrder = 0;     // SpriteRenderer ordering
    private SpriteRenderer sr;                // cached if present

    void Awake()
    {
        arc = GetComponentInChildren<ArcRenderer>(true);
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        isUI = (rectTransform != null && canvas != null);

        cam = Camera.main;
        originalScale = transform.localScale;
        originalLocalPos = transform.localPosition;
        originalRotation = transform.localRotation;

        sr = GetComponent<SpriteRenderer>();
        if (sr) originalSortingOrder = sr.sortingOrder;

        mousePointer = InputSystem.actions?.FindAction("Point");
        if (mousePointer != null && !mousePointer.enabled) mousePointer.Enable();

        if (glowEffect) glowEffect.SetActive(false);
        arc?.Show(false);
    }

    void OnDisable()
    {
        if (mousePointer != null && mousePointer.enabled) mousePointer.Disable();
        arc?.Show(false);
        Elevate(false);
    }

    void Update()
    {
        switch (currentState)
        {
            case 1:
                HandleHoverState();
                break;
            case 2:
                HandleDragState();
                // Fallback: if somehow mouse up isn't delivered, reset here
                if (Mouse.current == null || !Mouse.current.leftButton.IsPressed())
                    TransitionToState0();
                break;
        }
    }

    private void TransitionToState0()
    {
        currentState = 0;
        transform.localScale = originalScale;
        transform.localRotation = originalRotation;
        transform.localPosition = originalLocalPos;
        if (glowEffect) glowEffect.SetActive(false);
        arc?.Show(false);
        Elevate(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentState == 0)
        {
            originalLocalPos = transform.localPosition;
            originalRotation = transform.localRotation;
            originalScale = transform.localScale;

            Elevate(true);          // raise and bring-to-front
            currentState = 1;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentState == 1)
        {
            Elevate(false);
            TransitionToState0();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (currentState != 1) return;

        currentState = 2;

        // keep elevated; disable hover-only visuals; show arc
        Elevate(true);
        if (glowEffect) glowEffect.SetActive(false);
        arc?.Show(true);

        // cache (no movement now, but harmless to keep)
        Vector3 sp = eventData.position;
        sp.z = Mathf.Abs(transform.position.z - cam.transform.position.z);
        originalWorldPointer = cam.ScreenToWorldPoint(sp);
        originalWorldPos = transform.position;
    }

    public void OnPointerUp(PointerEventData eventData)   // ← NEW: explicit end
    {
        if (currentState == 2)
            TransitionToState0();
    }

    public void OnEndDrag(PointerEventData eventData)     // ← NEW: explicit end
    {
        if (currentState == 2)
            TransitionToState0();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (currentState != 2) return;

        // DO NOT move during drag; card stays at elevated hover pos.
        // ArcRenderer handles the cursor line independently.
    }

    private void HandleHoverState()
    {
        if (glowEffect && !glowEffect.activeSelf) glowEffect.SetActive(true);
        transform.localScale = originalScale * selectScale;
        Elevate(true); // safe if already elevated
    }

    private void HandleDragState()
    {
        // Keep orientation stable; keep arc visible.
        transform.localRotation = Quaternion.identity;
        arc?.Show(true);
        // No position changes here: card stays where hover placed it.
    }

    // ---------------- Elevation helper ----------------
    private void Elevate(bool on)
    {
        if (on == elevated) return;

        if (on)
        {
            // Lift in Y and Z
            var p = originalLocalPos;
            p.y += hoverYOffset;
            p.z += hoverZOffset;
            transform.localPosition = p;

            // Bring to front visually
            if (isUI && rectTransform)
            {
                if (originalSiblingIndex < 0 && transform.parent != null)
                    originalSiblingIndex = transform.GetSiblingIndex();
                transform.SetAsLastSibling();
            }
            else if (sr)
            {
                originalSortingOrder = sr.sortingOrder;
                sr.sortingOrder = originalSortingOrder + sortingOrderBoost;
            }
        }
        else
        {
            // Return to original position and ordering
            transform.localPosition = originalLocalPos;

            if (isUI && rectTransform && originalSiblingIndex >= 0 && transform.parent != null)
                transform.SetSiblingIndex(originalSiblingIndex);
            else if (sr)
                sr.sortingOrder = originalSortingOrder;
        }

        elevated = on;
    }
    // ---------------------------------------------------
}
