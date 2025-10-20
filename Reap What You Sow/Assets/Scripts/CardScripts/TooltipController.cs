using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TooltipController : MonoBehaviour
{
    public static TooltipController I;

    [Header("Refs")]
    public Canvas canvas;                 // your tooltip canvas (Screen Space - Camera recommended)
    public RectTransform panel;           // the tooltip panel
    public TextMeshProUGUI text;

    [Header("Tuning")]
    public Vector2 screenOffset = new(16f, -16f); // px away from cursor
    public bool pixelSnap = true;
    public bool autoFollowWhileVisible = true;    // follow mouse every frame (helps on WebGL)

    [Header("Optional")]
    public CanvasGroup cg;                // leave null if you didn’t add one

    Camera uiCam;
    RectTransform canvasRT;
    CanvasScaler scaler;
    Vector2 refRes;

    void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;

        if (!canvas) canvas = GetComponentInChildren<Canvas>(true);
        uiCam = canvas ? canvas.worldCamera : Camera.main;
        canvasRT = canvas ? canvas.transform as RectTransform : null;
        scaler = canvas ? canvas.GetComponent<CanvasScaler>() : null;
        refRes = scaler ? scaler.referenceResolution : new Vector2(320, 180);

        if (!cg) cg = GetComponent<CanvasGroup>();
        if (cg) { cg.alpha = 0f; cg.interactable = false; cg.blocksRaycasts = false; }

        Hide();
    }

    void Update()
    {
        if (!autoFollowWhileVisible) return;
        if (!panel || !panel.gameObject.activeSelf) return;
        SetPositionFromScreen(Input.mousePosition);
    }

    public void Show(CardEditor def, bool upgraded, Vector2 screenPos)
    {
        if (!def || !panel || !text || canvasRT == null) return;

        panel.gameObject.SetActive(true);
        if (cg) { cg.alpha = 1f; cg.interactable = false; cg.blocksRaycasts = false; }

        text.text = BuildText(def, upgraded);
        SetPositionFromScreen(screenPos);
    }

    public void Follow(Vector2 screenPos)  // still used by your trigger; now robust
    {
        if (!panel || !panel.gameObject.activeSelf) return;
        SetPositionFromScreen(screenPos);
    }

    public void Hide()
    {
        if (cg) { cg.alpha = 0f; cg.interactable = false; cg.blocksRaycasts = false; }
        if (panel) panel.gameObject.SetActive(false);
    }

    // --- Positioning that actually respects real canvas size & pivot ---
    void SetPositionFromScreen(Vector2 screenPos)
    {
        if (canvasRT == null) return;

        // 1) Convert cursor to local canvas space
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, screenPos, uiCam, out var local);

        // 2) Convert offset from reference px → local canvas units
        Vector2 canvasSize = canvasRT.rect.size;                 // real size (px in canvas space)
        Vector2 scale = new Vector2(canvasSize.x / refRes.x, canvasSize.y / refRes.y);
        Vector2 localOffset = new Vector2(screenOffset.x * scale.x, screenOffset.y * scale.y);

        Vector2 desired = local + localOffset;

        // 3) Clamp inside canvas, considering panel size & pivot
        Vector2 pSize = panel.sizeDelta;                         // in canvas units
        Vector2 halfCanvas = canvasSize * 0.5f;
        float minX = -halfCanvas.x + pSize.x * panel.pivot.x;
        float maxX = halfCanvas.x - pSize.x * (1f - panel.pivot.x);
        float minY = -halfCanvas.y + pSize.y * panel.pivot.y;
        float maxY = halfCanvas.y - pSize.y * (1f - panel.pivot.y);

        desired.x = Mathf.Clamp(desired.x, minX, maxX);
        desired.y = Mathf.Clamp(desired.y, minY, maxY);

        if (pixelSnap)
        {
            desired.x = Mathf.Round(desired.x);
            desired.y = Mathf.Round(desired.y);
        }

        panel.anchoredPosition = desired;
    }

string BuildText(CardEditor def, bool upgraded)
    {
        string name = string.IsNullOrEmpty(def.displayName) ? def.name : def.displayName;

        // Spells (unchanged, compact)
        if (def.isSpell)
        {
            int eSpell = upgraded ? def.upgradedEnergy : def.baseEnergy;
            string spell = def.spellKind == CardEditor.SpellKind.RemoveTargetCrop
                ? "Spell: Remove crop"
                : $"Spell: +{def.spellAmount} Energy";
            return $"{name}\nE:{eSpell}  [{spell}]";
        }

        // Crops (Treat/Trick candy is same baseline; we show one 'C')
        int eCost = upgraded ? def.upgradedEnergy : def.baseEnergy;
        int life = upgraded ? def.upgradedLifetime : def.baseLifetime;
        int candy = upgraded ? def.upgradedTreatCandy : def.baseTreatCandy;

        // Default per-neighbor text if no overrides
        int tAdj = upgraded ? def.upgradedTreatAdjPerNeighbor : def.baseTreatAdjPerNeighbor;
        int kAdj = upgraded ? def.upgradedTrickAdjPerNeighbor : def.baseTrickAdjPerNeighbor;

        bool flipT = def.treatFlipNeighbors;
        bool flipK = def.trickFlipNeighbors;
        int auraT = def.treatAuraToNeighbors;
        int auraK = def.trickAuraToNeighbors;

        var sb = new System.Text.StringBuilder(96);
        sb.AppendLine(name);
        sb.AppendLine($"E:{eCost} L:{life} C:{candy}");

        // Treat line
        if (!def.hideTreatLine)
        {
            string treatCore = !string.IsNullOrWhiteSpace(def.treatTextOverride)
                ? def.treatTextOverride
                : FormatAdj(tAdj); // e.g., "+1/nbr" or "+0"

            sb.Append("Treat: ").Append(treatCore).Append(FormatTags(flipT, auraT)).AppendLine();
        }

        // Trick line
        if (!def.hideTrickLine)
        {
            string trickCore = !string.IsNullOrWhiteSpace(def.trickTextOverride)
                ? def.trickTextOverride
                : FormatAdj(kAdj);

            sb.Append("Trick: ").Append(trickCore).Append(FormatTags(flipK, auraK));
        }

        return sb.ToString();
    }

    // Helpers unchanged:
    string FormatAdj(int perNeighbor)
    {
        if (perNeighbor == 0) return "+0";
        string sign = perNeighbor > 0 ? "+" : "−";
        int mag = Mathf.Abs(perNeighbor);
        return $"{sign}{mag}/nbr";
    }
    string FormatTags(bool flip, int aura)
    {
        if (!flip && aura == 0) return string.Empty;
        if (flip && aura != 0)
            return aura > 0 ? $"  [Flip][+{aura} aura]" : $"  [Flip][{aura} aura]";
        if (flip) return "  [Flip]";
        return aura > 0 ? $"  [+{aura} aura]" : $"  [{aura} aura]";
    }

    Vector2 ScreenToCanvas(Vector2 screenPos)
    {
        // Convert screen px → canvas anchored pos (Screen Space - Camera or Overlay)
        RectTransform canvasRT = canvas.transform as RectTransform;
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, screenPos, uiCam, out local);
        return local;
    }

    Vector2 ClampToScreen(Vector2 screenPos, Vector2 panelSize)
    {
        // Clamp so the panel stays fully inside the reference resolution
        float x = Mathf.Clamp(screenPos.x, 0f, refRes.x - panelSize.x);
        float y = Mathf.Clamp(screenPos.y, 0f, refRes.y - panelSize.y);
        return new Vector2(x, y);
    }
}

