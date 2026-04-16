using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Top-right folder panel showing every collected clue.
/// Toggle open / closed with E.
///
/// Each clue entry has a "Present" button that is only interactive while
/// Julian's dialogue is open. Clicking it calls JulianDialogue.Instance.PresentClue().
///
/// Assign PanelRoot and ClueListContent from the scene Canvas in the Inspector.
/// </summary>
public class ClueInventoryUI : MonoBehaviour
{
    public static ClueInventoryUI Instance { get; private set; }

    /// <summary>True while the clue folder is open.</summary>
    public static bool IsOpen { get; private set; }

    [Header("UI References — assign from scene Canvas")]
    [Tooltip("The root panel to show/hide when the player presses E.")]
    public GameObject PanelRoot;

    [Tooltip("The scroll content transform that clue entries are added to.")]
    public RectTransform ClueListContent;

    // Tracks which clue entry GameObjects have been spawned.
    private readonly List<GameObject> entryRows = new List<GameObject>();

    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        ClueInventory.OnClueAdded += OnClueAdded;
    }

    private void OnDisable()
    {
        ClueInventory.OnClueAdded -= OnClueAdded;
    }

    private void Start()
    {
        if (PanelRoot != null) PanelRoot.SetActive(false);

        // Populate any clues already in the inventory (e.g. from a save).
        if (ClueInventory.Instance != null)
        {
            foreach (string clue in ClueInventory.Instance.Clues)
                SpawnEntry(clue);
        }
    }

    private void Update()
    {
        // Don't let E toggle the folder while Julian's VN screen is open;
        // instead the folder is shown via ShowForPresentation().
        if (JulianDialogue.IsOpen) return;
        if (InspectionScreen.IsOpen) return;

        if (Input.GetKeyDown(KeyCode.E))
            SetOpen(!IsOpen);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Public API
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Opens the inventory in "presentation mode" during Julian's dialogue.
    /// Present buttons are always active; the folder closes when a clue is chosen
    /// or when the player presses E.
    /// </summary>
    public void ShowForPresentation()
    {
        RefreshPresentButtons();
        SetOpen(true);
    }

    /// <summary>Refreshes Present button interactability and closes if no clues exist.</summary>
    public void RefreshPresentButtons()
    {
        bool canPresent = JulianDialogue.IsOpen;
        foreach (GameObject row in entryRows)
        {
            Button btn = row.GetComponentInChildren<Button>();
            if (btn != null) btn.interactable = canPresent;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Internal
    // ─────────────────────────────────────────────────────────────────────────

    private void SetOpen(bool open)
    {
        IsOpen = open;
        if (PanelRoot != null) PanelRoot.SetActive(open);

        if (open) RefreshPresentButtons();
    }

    private void OnClueAdded(string clueText)
    {
        SpawnEntry(clueText);
    }

    /// <summary>Creates a row in the clue list for the given clue text.</summary>
    private void SpawnEntry(string clueText)
    {
        if (ClueListContent == null) return;

        GameObject row = new GameObject("ClueEntry");
        row.transform.SetParent(ClueListContent, false);

        // Give the row a layout element so the vertical layout sizes it correctly.
        LayoutElement le = row.AddComponent<LayoutElement>();
        le.minHeight      = 72f;
        le.preferredHeight = 72f;
        le.flexibleWidth  = 1f;

        Image rowBg = row.AddComponent<Image>();
        rowBg.color = new Color(0.10f, 0.10f, 0.14f, 1f);

        // Horizontal layout: clue text | Present button
        HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment       = TextAnchor.MiddleLeft;
        hlg.childControlHeight   = true;
        hlg.childControlWidth    = false;
        hlg.childForceExpandHeight = true;
        hlg.padding = new RectOffset(12, 8, 8, 8);
        hlg.spacing = 10f;

        // Clue text
        GameObject textGO = new GameObject("ClueText");
        textGO.transform.SetParent(row.transform, false);
        LayoutElement textLE = textGO.AddComponent<LayoutElement>();
        textLE.flexibleWidth = 1f;

        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text             = clueText;
        tmp.fontSize         = 20f;
        tmp.fontStyle        = FontStyles.Italic;
        tmp.color            = new Color(0.95f, 0.90f, 0.72f, 1f);
        tmp.alignment        = TextAlignmentOptions.MidlineLeft;
        tmp.enableWordWrapping = true;

        // Present button (fixed width)
        GameObject btnGO = new GameObject("PresentBtn");
        btnGO.transform.SetParent(row.transform, false);
        LayoutElement btnLE = btnGO.AddComponent<LayoutElement>();
        btnLE.minWidth       = 110f;
        btnLE.preferredWidth = 110f;

        Image btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.65f, 0.45f, 0.10f, 1f);

        Button btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        btn.interactable  = false;     // enabled only inside Julian's dialogue

        ColorBlock cb = btn.colors;
        cb.normalColor      = new Color(0.65f, 0.45f, 0.10f, 1f);
        cb.highlightedColor = new Color(0.85f, 0.60f, 0.15f, 1f);
        cb.pressedColor     = new Color(0.45f, 0.30f, 0.06f, 1f);
        cb.disabledColor    = new Color(0.35f, 0.35f, 0.35f, 0.5f);
        btn.colors = cb;

        string captured = clueText;      // capture for lambda
        btn.onClick.AddListener(() => OnPresentClicked(captured));

        GameObject btnTextGO = new GameObject("Label");
        btnTextGO.transform.SetParent(btnGO.transform, false);
        TextMeshProUGUI btnLabel = btnTextGO.AddComponent<TextMeshProUGUI>();
        btnLabel.text      = "Present";
        btnLabel.fontSize  = 18f;
        btnLabel.fontStyle = FontStyles.Bold;
        btnLabel.color     = Color.white;
        btnLabel.alignment = TextAlignmentOptions.Center;
        RectTransform btnTextRT = btnTextGO.GetComponent<RectTransform>();
        btnTextRT.anchorMin = Vector2.zero;
        btnTextRT.anchorMax = Vector2.one;
        btnTextRT.offsetMin = new Vector2(4f, 4f);
        btnTextRT.offsetMax = new Vector2(-4f, -4f);

        entryRows.Add(row);
    }

    private void OnPresentClicked(string clueText)
    {
        SetOpen(false);
    }
}
