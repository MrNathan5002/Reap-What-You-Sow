using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CardMovement : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform rectTransform;      // present if UI
    private Canvas canvas;                    // present if UI
    private bool isUI;                        // UI mode vs world-sprite mode

    private Vector2 originalLocalPointerPosition;  // UI drag start (canvas space)
    private Vector2 originalPanelAnchoredPos;      // UI anchored start

    private Vector3 originalWorldPointer;          // world drag start (world space)
    private Vector3 originalWorldPos;              // world start pos

    private Vector3 originalScale;
    private int currentState = 0;
    private Quaternion originalRotation;
    private Vector3 originalLocalPos;
    private InputAction mousePointer;
    private Camera cam;                        // main/event camera

    [SerializeField] private float selectScale = 1.1f;
    [SerializeField] private Vector2 cardPlay;
    [SerializeField] private Vector3 playPosition;
    [SerializeField] private GameObject glowEffect;
    [SerializeField] private GameObject playArrow;

    private ArcRenderer arc; // ← the arc on this card only

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

        mousePointer = InputSystem.actions?.FindAction("Point");
        if (mousePointer != null && !mousePointer.enabled) mousePointer.Enable();

        if (glowEffect) glowEffect.SetActive(false);
        if (playArrow) playArrow.SetActive(false);

        // --- NEW: ensure the arc starts hidden ---
        arc?.Show(false);
    }

    void OnDisable()
    {
        if (mousePointer != null && mousePointer.enabled) mousePointer.Disable();
        // --- NEW: hide arc if this object gets disabled mid-drag ---
        arc?.Show(false);
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
                if (Mouse.current == null || !Mouse.current.leftButton.IsPressed())
                    TransitionToState0();
                break;
            case 3:
                HandlePlayState();
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
        if (playArrow) playArrow.SetActive(false);

        // --- NEW: hide arc when drag/play ends ---
        arc?.Show(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentState == 0)
        {
            originalLocalPos = transform.localPosition;
            originalRotation = transform.localRotation;
            originalScale = transform.localScale;
            currentState = 1;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentState == 1)
            TransitionToState0();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (currentState != 1) return;

        currentState = 2;

        // --- NEW: show arc when dragging begins ---
        arc?.Show(true);

        if (isUI)
        {
            var canvasRect = canvas.GetComponent<RectTransform>();
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, eventData.pressEventCamera, out originalLocalPointerPosition))
            {
                originalPanelAnchoredPos = rectTransform.anchoredPosition;
            }
        }
        else
        {
            // World-sprite mode: cache world pointer and start pos
            Vector3 sp = eventData.position;
            sp.z = Mathf.Abs(transform.position.z - cam.transform.position.z);
            originalWorldPointer = cam.ScreenToWorldPoint(sp);
            originalWorldPos = transform.position;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (currentState != 2) return;

        if (isUI)
        {
            var canvasRect = canvas.GetComponent<RectTransform>();
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, eventData.pressEventCamera, out var localPointer))
            {
                Vector2 delta = localPointer - originalLocalPointerPosition;
                rectTransform.anchoredPosition = originalPanelAnchoredPos + delta;

                if (transform.localPosition.y > cardPlay.y)
                {
                    currentState = 3;
                    if (playArrow) playArrow.SetActive(true);
                    transform.localPosition = playPosition;
                    // keep arc visible while in play state (still holding mouse)
                    arc?.Show(true);
                }
            }
        }
        else
        {
            // World-sprite dragging in world units
            Vector3 sp = eventData.position;
            sp.z = Mathf.Abs(transform.position.z - cam.transform.position.z);
            Vector3 wp = cam.ScreenToWorldPoint(sp);

            Vector3 delta = wp - originalWorldPointer;
            transform.position = originalWorldPos + delta;

            if (transform.localPosition.y > cardPlay.y)
            {
                currentState = 3;
                if (playArrow) playArrow.SetActive(true);
                transform.localPosition = playPosition;
                // keep arc visible while in play state (still holding mouse)
                arc?.Show(true);
            }
        }
    }

    private void HandleHoverState()
    {
        if (glowEffect && !glowEffect.activeSelf) glowEffect.SetActive(true);
        transform.localScale = originalScale * selectScale;
    }

    private void HandleDragState()
    {
        transform.localRotation = Quaternion.identity;
        // ensure arc remains on during drag (in case anything toggled it)
        arc?.Show(true);
    }

    private void HandlePlayState()
    {
        transform.localPosition = playPosition;
        transform.localRotation = Quaternion.identity;

        var pointer = (mousePointer != null) ? mousePointer.ReadValue<Vector2>() : Vector2.zero;
        if (pointer.y < cardPlay.y)
        {
            currentState = 2;
            if (playArrow) playArrow.SetActive(false);
            // still dragging → keep arc on
            arc?.Show(true);
        }
    }
}
