using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Full-screen visual-novel dialogue for Julian.
/// Builds its own Canvas at runtime — no prefabs or extra scenes required.
///
/// Flow:
///   Open()  → Julian says "What?" with two choices:
///     [You are guilty! I found a clue!] → sprite swaps to JulianSprite2 (scared), auto-closes.
///     [Nevermind.]                       → immediate close.
/// </summary>
public class JulianDialogue : MonoBehaviour
{
    public static JulianDialogue Instance { get; private set; }

    /// <summary>True while the VN screen is visible.</summary>
    public static bool IsOpen { get; private set; }

    // ── Inspector ──────────────────────────────────────────────────────────────

    [Header("Julian Sprites")]
    [Tooltip("Default portrait shown when dialogue opens.")]
    public Sprite JulianSprite1;

    [Tooltip("Scared portrait shown when accused.")]
    public Sprite JulianSprite2;

    [Header("Scene Reference")]
    [Tooltip("Julian's SpriteRenderer in the world (swapped alongside the portrait).")]
    public SpriteRenderer JulianSpriteRenderer;

    // ── Dialogue strings ───────────────────────────────────────────────────────

    private const string CharacterName   = "Julian";
    private const string GreetingLine    = "What?";
    private const string ScaredResponse  = "W-what?! That wasn't me! You've got the wrong person!";
    private const string ChoiceAccuse    = "You are guilty! I found a clue!";
    private const string ChoiceNevermind = "Nevermind.";
    private const float  AutoCloseDelay  = 3.5f;

    // ── Runtime UI refs ────────────────────────────────────────────────────────

    private GameObject      vnRoot;
    private Image           portraitImage;
    private TextMeshProUGUI nameLabel;
    private TextMeshProUGUI bodyLabel;
    private GameObject      choicePanel;

    private FPSController fpsController;

    // ── Lifecycle ──────────────────────────────────────────────────────────────

    private void Awake()
    {
        Instance = this;
        EnsureEventSystem();
        BuildUI();
        vnRoot.SetActive(false);
    }

    private void Start()
    {
        fpsController = FindObjectOfType<FPSController>();
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>Opens the VN screen and starts the conversation with Julian.</summary>
    public void Open()
    {
        if (IsOpen) return;
        IsOpen = true;

        SetPlayerLocked(true);
        SetPortrait(JulianSprite1);
        if (JulianSpriteRenderer != null && JulianSprite1 != null)
            JulianSpriteRenderer.sprite = JulianSprite1;

        SetDialogue(CharacterName, GreetingLine);
        choicePanel.SetActive(true);
        vnRoot.SetActive(true);
    }

    /// <summary>Closes the VN screen and restores player control.</summary>
    public void Close()
    {
        if (!IsOpen) return;
        StopAllCoroutines();
        vnRoot.SetActive(false);
        IsOpen = false;
        SetPlayerLocked(false);
    }

    // ── Choice handlers ────────────────────────────────────────────────────────

    private void OnAccuseChosen()
    {
        choicePanel.SetActive(false);

        SetPortrait(JulianSprite2);
        if (JulianSpriteRenderer != null && JulianSprite2 != null)
            JulianSpriteRenderer.sprite = JulianSprite2;

        SetDialogue(CharacterName, ScaredResponse);
        StartCoroutine(AutoCloseRoutine());
    }

    private void OnNevermindChosen() => Close();

    private IEnumerator AutoCloseRoutine()
    {
        yield return new WaitForSeconds(AutoCloseDelay);
        Close();
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private void SetPortrait(Sprite sprite)
    {
        if (portraitImage == null) return;
        if (sprite == null) { portraitImage.enabled = false; return; }
        portraitImage.sprite         = sprite;
        portraitImage.preserveAspect = true;
        portraitImage.enabled        = true;
    }

    private void SetDialogue(string speaker, string line)
    {
        if (nameLabel != null) nameLabel.text = speaker.ToUpper();
        if (bodyLabel  != null) bodyLabel.text  = line;
    }

    private void SetPlayerLocked(bool locked)
    {
        if (fpsController != null)
        {
            fpsController.canMove = !locked;
            fpsController.canLook = !locked;
        }
        Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible   = locked;
    }

    // ── EventSystem guard ──────────────────────────────────────────────────────

    private static void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null) return;
        GameObject es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
    }

    // ── UI construction ────────────────────────────────────────────────────────

    private void BuildUI()
    {
        // Canvas — sortingOrder 300 guarantees it renders above everything else
        GameObject canvasGO = new GameObject("JulianVNCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 300;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Full-screen dark backdrop
        vnRoot = new GameObject("VNRoot");
        vnRoot.transform.SetParent(canvasGO.transform, false);
        vnRoot.AddComponent<Image>().color = new Color(0.04f, 0.04f, 0.07f, 1f);
        AnchorFill(vnRoot.GetComponent<RectTransform>());

        // Portrait — upper portion of screen, centered
        GameObject portraitGO = new GameObject("Portrait");
        portraitGO.transform.SetParent(vnRoot.transform, false);
        portraitImage = portraitGO.AddComponent<Image>();
        portraitImage.preserveAspect = true;
        RectTransform portraitRT = portraitGO.GetComponent<RectTransform>();
        portraitRT.anchorMin = new Vector2(0.25f, 0.32f);
        portraitRT.anchorMax = new Vector2(0.75f, 1.00f);
        portraitRT.offsetMin = portraitRT.offsetMax = Vector2.zero;

        // Dialogue panel — bottom 32 %
        GameObject panel = new GameObject("DialoguePanel");
        panel.transform.SetParent(vnRoot.transform, false);
        panel.AddComponent<Image>().color = new Color(0.06f, 0.06f, 0.12f, 0.97f);
        RectTransform panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0f, 0f);
        panelRT.anchorMax = new Vector2(1f, 0.32f);
        panelRT.offsetMin = panelRT.offsetMax = Vector2.zero;

        // Gold accent bar
        GameObject accent = new GameObject("AccentBar");
        accent.transform.SetParent(panel.transform, false);
        accent.AddComponent<Image>().color = new Color(0.95f, 0.75f, 0.20f, 1f);
        RectTransform accentRT = accent.GetComponent<RectTransform>();
        accentRT.anchorMin = new Vector2(0f, 1f); accentRT.anchorMax = new Vector2(1f, 1f);
        accentRT.offsetMin = new Vector2(0f, -5f); accentRT.offsetMax = Vector2.zero;

        // Name plate
        GameObject namePlate = new GameObject("NamePlate");
        namePlate.transform.SetParent(panel.transform, false);
        namePlate.AddComponent<Image>().color = new Color(0.95f, 0.75f, 0.20f, 1f);
        RectTransform npRT = namePlate.GetComponent<RectTransform>();
        npRT.anchorMin = new Vector2(0f, 1f); npRT.anchorMax = new Vector2(0f, 1f);
        npRT.pivot = new Vector2(0f, 0f);
        npRT.anchoredPosition = new Vector2(28f, 6f);
        npRT.sizeDelta = new Vector2(230f, 46f);

        GameObject nameTextGO = new GameObject("NameText");
        nameTextGO.transform.SetParent(namePlate.transform, false);
        nameLabel = nameTextGO.AddComponent<TextMeshProUGUI>();
        nameLabel.text          = CharacterName.ToUpper();
        nameLabel.fontSize      = 24f;
        nameLabel.fontStyle     = FontStyles.Bold | FontStyles.SmallCaps;
        nameLabel.color         = new Color(0.08f, 0.06f, 0.04f, 1f);
        nameLabel.alignment     = TextAlignmentOptions.Center;
        nameLabel.raycastTarget = false;
        AnchorFill(nameTextGO.GetComponent<RectTransform>(), new Vector2(10f, 4f), new Vector2(-10f, -4f));

        // Dialogue body text
        GameObject bodyGO = new GameObject("BodyText");
        bodyGO.transform.SetParent(panel.transform, false);
        bodyLabel = bodyGO.AddComponent<TextMeshProUGUI>();
        bodyLabel.fontSize           = 30f;
        bodyLabel.color              = new Color(0.95f, 0.92f, 0.88f, 1f);
        bodyLabel.alignment          = TextAlignmentOptions.TopLeft;
        bodyLabel.enableWordWrapping = true;
        bodyLabel.raycastTarget      = false;
        RectTransform bodyRT = bodyGO.GetComponent<RectTransform>();
        bodyRT.anchorMin = new Vector2(0f, 0.44f); bodyRT.anchorMax = new Vector2(1f, 0.96f);
        bodyRT.offsetMin = new Vector2(36f, 0f);   bodyRT.offsetMax = new Vector2(-36f, 0f);

        // Choice panel — two buttons side by side in the lower part of the dialogue panel
        choicePanel = new GameObject("ChoicePanel");
        choicePanel.transform.SetParent(panel.transform, false);
        RectTransform choiceRT = choicePanel.AddComponent<RectTransform>();
        choiceRT.anchorMin = new Vector2(0f, 0.02f); choiceRT.anchorMax = new Vector2(1f, 0.42f);
        choiceRT.offsetMin = new Vector2(28f, 6f);   choiceRT.offsetMax = new Vector2(-28f, -6f);

        HorizontalLayoutGroup hlg = choicePanel.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 24f;
        hlg.childAlignment        = TextAnchor.MiddleCenter;
        hlg.childControlWidth     = hlg.childControlHeight    = true;
        hlg.childForceExpandWidth = hlg.childForceExpandHeight = true;
        hlg.padding = new RectOffset(0, 0, 4, 4);

        MakeButton(choicePanel.transform, ChoiceAccuse,
            new Color(0.60f, 0.10f, 0.08f, 1f), OnAccuseChosen);
        MakeButton(choicePanel.transform, ChoiceNevermind,
            new Color(0.18f, 0.20f, 0.28f, 1f), OnNevermindChosen);
    }

    // ── Layout helpers ─────────────────────────────────────────────────────────

    private static void AnchorFill(RectTransform rt,
        Vector2 offsetMin = default, Vector2 offsetMax = default)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = offsetMin;   rt.offsetMax = offsetMax;
    }

    private static void MakeButton(Transform parent, string labelText,
        Color bgColor, UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = new GameObject("Btn_" + labelText[..Mathf.Min(12, labelText.Length)]);
        go.transform.SetParent(parent, false);

        Image img = go.AddComponent<Image>();
        img.color = bgColor;

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        ColorBlock cb = btn.colors;
        cb.normalColor      = bgColor;
        cb.highlightedColor = bgColor + new Color(0.15f, 0.15f, 0.15f, 0f);
        cb.pressedColor     = bgColor * 0.65f;
        cb.selectedColor    = bgColor;
        btn.colors = cb;
        btn.onClick.AddListener(onClick);

        GameObject textGO = new GameObject("Label");
        textGO.transform.SetParent(go.transform, false);
        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text               = labelText;
        tmp.fontSize           = 24f;
        tmp.fontStyle          = FontStyles.Bold;
        tmp.color              = Color.white;
        tmp.alignment          = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = true;
        tmp.raycastTarget      = false;
        AnchorFill(textGO.GetComponent<RectTransform>(), new Vector2(14f, 6f), new Vector2(-14f, -6f));
    }
}
