using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Drives the Clue Board scene (ClueBoard.unity).
/// Add this component to a ClueBoardManager GameObject in that scene.
///
/// The scene is loaded additively over the puzzle scene so ClueInventory
/// (which lives in puzzle) stays alive and accessible.
///
/// Layout: dark full-screen background, scrollable grid of clue cards,
/// and a Close button that unloads this scene.
/// </summary>
public class ClueBoardScene : MonoBehaviour
{
    private const string PuzzleSceneName = "puzzle";

    private Transform cardContainer;

    private void Start()
    {
        BuildUI();
        PopulateClues();
    }

    // ── UI ─────────────────────────────────────────────────────────────────────

    private void BuildUI()
    {
        // Canvas
        GameObject canvasGO = new GameObject("ClueBoardCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 400;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Full-screen dark background
        GameObject root = new GameObject("Root");
        root.transform.SetParent(canvasGO.transform, false);
        Image bg = root.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.05f, 0.08f, 1f);
        Stretch(root.GetComponent<RectTransform>());

        // Header bar
        GameObject header = new GameObject("Header");
        header.transform.SetParent(root.transform, false);
        header.AddComponent<Image>().color = new Color(0.95f, 0.75f, 0.20f, 1f);
        RectTransform hRT = header.GetComponent<RectTransform>();
        hRT.anchorMin = new Vector2(0f, 0.90f); hRT.anchorMax = Vector2.one;
        hRT.offsetMin = hRT.offsetMax = Vector2.zero;

        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(header.transform, false);
        TextMeshProUGUI title = titleGO.AddComponent<TextMeshProUGUI>();
        title.text      = "CLUE BOARD";
        title.fontSize  = 36f;
        title.fontStyle = FontStyles.Bold | FontStyles.SmallCaps;
        title.color     = new Color(0.08f, 0.06f, 0.04f, 1f);
        title.alignment = TextAlignmentOptions.Center;
        title.raycastTarget = false;
        Stretch(titleGO.GetComponent<RectTransform>(), new Vector2(20f, 6f), new Vector2(-20f, -6f));

        // Close button (top-right corner)
        GameObject closeBtnGO = new GameObject("CloseBtn");
        closeBtnGO.transform.SetParent(header.transform, false);
        RectTransform closeBtnRT = closeBtnGO.AddComponent<RectTransform>();
        closeBtnRT.anchorMin = new Vector2(0.88f, 0.1f); closeBtnRT.anchorMax = new Vector2(0.98f, 0.9f);
        closeBtnRT.offsetMin = closeBtnRT.offsetMax = Vector2.zero;

        Image closeBtnImg = closeBtnGO.AddComponent<Image>();
        closeBtnImg.color = new Color(0.55f, 0.12f, 0.08f, 1f);
        Button closeBtn = closeBtnGO.AddComponent<Button>();
        closeBtn.targetGraphic = closeBtnImg;
        ColorBlock closeCB = closeBtn.colors;
        closeCB.highlightedColor = new Color(0.72f, 0.20f, 0.12f, 1f);
        closeCB.pressedColor     = new Color(0.38f, 0.08f, 0.05f, 1f);
        closeBtn.colors = closeCB;
        closeBtn.onClick.AddListener(OnClose);

        GameObject closeLabelGO = new GameObject("Label");
        closeLabelGO.transform.SetParent(closeBtnGO.transform, false);
        TextMeshProUGUI closeLabel = closeLabelGO.AddComponent<TextMeshProUGUI>();
        closeLabel.text         = "✕ Close";
        closeLabel.fontSize     = 22f;
        closeLabel.fontStyle    = FontStyles.Bold;
        closeLabel.color        = Color.white;
        closeLabel.alignment    = TextAlignmentOptions.Center;
        closeLabel.raycastTarget = false;
        Stretch(closeLabelGO.GetComponent<RectTransform>(), new Vector2(6f, 4f), new Vector2(-6f, -4f));

        // Scroll view — fills between header and bottom margin
        GameObject scrollGO = new GameObject("ScrollView");
        scrollGO.transform.SetParent(root.transform, false);
        scrollGO.AddComponent<Image>().color = Color.clear;
        RectTransform scrollRT = scrollGO.GetComponent<RectTransform>();
        scrollRT.anchorMin = new Vector2(0f, 0f); scrollRT.anchorMax = new Vector2(1f, 0.90f);
        scrollRT.offsetMin = new Vector2(30f, 20f); scrollRT.offsetMax = new Vector2(-30f, -10f);

        // Grid content
        GameObject contentGO = new GameObject("Content");
        contentGO.transform.SetParent(scrollGO.transform, false);
        RectTransform contentRT = contentGO.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0f, 1f); contentRT.anchorMax = new Vector2(1f, 1f);
        contentRT.pivot     = new Vector2(0.5f, 1f);
        contentRT.offsetMin = contentRT.offsetMax = Vector2.zero;
        cardContainer = contentGO.transform;

        GridLayoutGroup grid = contentGO.AddComponent<GridLayoutGroup>();
        grid.cellSize        = new Vector2(520f, 160f);
        grid.spacing         = new Vector2(20f, 20f);
        grid.startCorner     = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis       = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment  = TextAnchor.UpperLeft;
        grid.constraint      = GridLayoutGroup.Constraint.Flexible;
        grid.padding         = new RectOffset(10, 10, 10, 10);

        ContentSizeFitter csf = contentGO.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        csf.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect sr = scrollGO.AddComponent<ScrollRect>();
        sr.content    = contentRT;
        sr.horizontal = false; sr.vertical = true;
        sr.movementType      = ScrollRect.MovementType.Clamped;
        sr.scrollSensitivity = 20f;

        // Empty state label — hidden once cards are spawned
        GameObject emptyGO = new GameObject("EmptyLabel");
        emptyGO.transform.SetParent(root.transform, false);
        TextMeshProUGUI emptyTmp = emptyGO.AddComponent<TextMeshProUGUI>();
        emptyTmp.text       = "No clues collected yet.\nExplore the scene and inspect objects to find evidence.";
        emptyTmp.fontSize   = 28f;
        emptyTmp.fontStyle  = FontStyles.Italic;
        emptyTmp.color      = new Color(0.50f, 0.50f, 0.55f, 1f);
        emptyTmp.alignment  = TextAlignmentOptions.Center;
        emptyTmp.enableWordWrapping = true;
        emptyTmp.raycastTarget = false;
        RectTransform emptyRT = emptyGO.GetComponent<RectTransform>();
        emptyRT.anchorMin = new Vector2(0.1f, 0.3f); emptyRT.anchorMax = new Vector2(0.9f, 0.7f);
        emptyRT.offsetMin = emptyRT.offsetMax = Vector2.zero;

        // Hide empty label if there are clues
        bool hasClues = ClueInventory.Instance != null && ClueInventory.Instance.Clues.Count > 0;
        emptyGO.SetActive(!hasClues);
    }

    private void PopulateClues()
    {
        if (ClueInventory.Instance == null) return;

        foreach (string clue in ClueInventory.Instance.Clues)
            SpawnCard(clue);
    }

    private void SpawnCard(string clueText)
    {
        GameObject card = new GameObject("ClueCard");
        card.transform.SetParent(cardContainer, false);

        Image cardBg = card.AddComponent<Image>();
        cardBg.color = new Color(0.10f, 0.10f, 0.16f, 1f);

        // Left gold accent stripe
        GameObject stripe = new GameObject("Stripe");
        stripe.transform.SetParent(card.transform, false);
        stripe.AddComponent<Image>().color = new Color(0.95f, 0.75f, 0.20f, 1f);
        RectTransform stripeRT = stripe.GetComponent<RectTransform>();
        stripeRT.anchorMin = Vector2.zero; stripeRT.anchorMax = new Vector2(0f, 1f);
        stripeRT.offsetMin = Vector2.zero; stripeRT.offsetMax = new Vector2(8f, 0f);

        // Clue number badge
        int cardIndex = cardContainer.childCount;
        GameObject badgeGO = new GameObject("Badge");
        badgeGO.transform.SetParent(card.transform, false);
        RectTransform badgeRT = badgeGO.AddComponent<RectTransform>();
        badgeRT.anchorMin = new Vector2(0f, 0.5f); badgeRT.anchorMax = new Vector2(0f, 0.5f);
        badgeRT.pivot     = new Vector2(0f, 0.5f);
        badgeRT.anchoredPosition = new Vector2(16f, 0f);
        badgeRT.sizeDelta        = new Vector2(44f, 44f);

        Image badgeBg = badgeGO.AddComponent<Image>();
        badgeBg.color = new Color(0.95f, 0.75f, 0.20f, 0.18f);

        GameObject badgeLabelGO = new GameObject("Number");
        badgeLabelGO.transform.SetParent(badgeGO.transform, false);
        TextMeshProUGUI badgeTmp = badgeLabelGO.AddComponent<TextMeshProUGUI>();
        badgeTmp.text       = $"#{cardIndex}";
        badgeTmp.fontSize   = 17f;
        badgeTmp.fontStyle  = FontStyles.Bold;
        badgeTmp.color      = new Color(0.95f, 0.75f, 0.20f, 1f);
        badgeTmp.alignment  = TextAlignmentOptions.Center;
        badgeTmp.raycastTarget = false;
        Stretch(badgeLabelGO.GetComponent<RectTransform>());

        // Clue text
        GameObject textGO = new GameObject("ClueText");
        textGO.transform.SetParent(card.transform, false);
        TextMeshProUGUI clueLabel = textGO.AddComponent<TextMeshProUGUI>();
        clueLabel.text              = clueText;
        clueLabel.fontSize          = 22f;
        clueLabel.fontStyle         = FontStyles.Italic;
        clueLabel.color             = new Color(0.94f, 0.90f, 0.76f, 1f);
        clueLabel.alignment         = TextAlignmentOptions.MidlineLeft;
        clueLabel.enableWordWrapping = true;
        clueLabel.raycastTarget     = false;
        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero; textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(72f, 12f); textRT.offsetMax = new Vector2(-16f, -12f);
    }

    private void OnClose()
    {
        SceneManager.UnloadSceneAsync("ClueBoard");
    }

    private static void Stretch(RectTransform rt,
        Vector2 offsetMin = default, Vector2 offsetMax = default)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = offsetMin;   rt.offsetMax = offsetMax;
    }
}
