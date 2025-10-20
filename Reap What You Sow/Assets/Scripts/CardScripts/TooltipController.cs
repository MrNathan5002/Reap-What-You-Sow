using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TooltipController : MonoBehaviour
{
    public static TooltipController I;

    [Header("Refs")]
    public Canvas canvas;                // this canvas
    public RectTransform panel;          // background panel
    public TextMeshProUGUI text;         // content text

    [Header("Tuning")]
    public Vector2 screenOffset = new(8f, -8f); // px in reference res
    public bool pixelSnap = true;

    Camera uiCam;
    Vector2 refRes;

    void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;

        if (!canvas) canvas = GetComponentInChildren<Canvas>(true);
        uiCam = canvas ? canvas.worldCamera : Camera.main;

        // Try to read reference resolution from CanvasScaler (if present)
        var scaler = canvas ? canvas.GetComponent<CanvasScaler>() : null;
        refRes = scaler ? scaler.referenceResolution : new Vector2(320, 180);

        Hide();
    }

    public void Show(CardEditor def, bool upgraded, Vector2 screenPos)
    {
        if (!def || !panel || !text) return;
        panel.gameObject.SetActive(true);

        text.text = BuildText(def, upgraded);

        // position near cursor (clamp inside screen)
        Vector2 pos = screenPos + screenOffset;
        pos = ClampToScreen(pos, panel.sizeDelta);
        if (pixelSnap) pos = new Vector2(Mathf.Round(pos.x), Mathf.Round(pos.y));

        panel.anchoredPosition = ScreenToCanvas(pos);
    }

    public void Hide()
    {
        if (panel) panel.gameObject.SetActive(false);
    }

    // Call each frame while visible if you want true follow; or only on enter/move.
    public void Follow(Vector2 screenPos)
    {
        if (!panel || !panel.gameObject.activeSelf) return;
        Vector2 pos = screenPos + screenOffset;
        pos = ClampToScreen(pos, panel.sizeDelta);
        if (pixelSnap) pos = new Vector2(Mathf.Round(pos.x), Mathf.Round(pos.y));
        panel.anchoredPosition = ScreenToCanvas(pos);
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

