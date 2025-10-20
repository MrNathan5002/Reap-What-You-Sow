using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CardDisplay))]
public class TooltipTriggerCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    public bool isDraggingGuard = true; // don’t show while dragging

    CardDisplay disp;
    HandCard handCard;                  // to read isUpgraded
    CardMovement mover;                 // to know drag state

    bool hovering;

    void Awake()
    {
        disp = GetComponent<CardDisplay>();
        handCard = GetComponent<HandCard>();
        mover = GetComponent<CardMovement>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isDraggingGuard && mover != null && mover.IsDragging) return;
        hovering = true;
        ShowAt(eventData.position);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (!hovering) return;
        if (isDraggingGuard && mover != null && mover.IsDragging) { Hide(); return; }
        TooltipController.I?.Follow(eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
        Hide();
    }

    void ShowAt(Vector2 screenPos)
    {
        if (!disp || !disp.cardData) return;
        bool upgraded = handCard != null && handCard.instance != null && handCard.instance.isUpgraded;
        TooltipController.I?.Show(disp.cardData, upgraded, screenPos);
    }

    void Hide() => TooltipController.I?.Hide();
}
