using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Museum exhibit puzzle.
///
/// 1. Player left-clicks the display case to open the keypad.
/// 2. Type digits 0-9. Backspace deletes. Enter / auto on 4th digit confirms.
/// 3. Correct code → case disappears, the Artifact is left exposed.
/// 4. Player clicks the Artifact to collect it — a banner shows briefly,
///    then the artifact disappears and full first-person control returns.
/// </summary>
public class MuseumPuzzle : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────────

    [Header("Code")]
    public int[] SecretCode = { 1, 9, 4, 2 };

    [Header("Scene references")]
    public FPSController PlayerController;

    [Tooltip("The gold sphere artifact that is revealed and collected.")]
    public GameObject Artifact;

    [Header("Objects to hide when solved (case body is hidden automatically)")]
    [Tooltip("Drag CasePlinth, CaseLidPivot, ArtifactRiser here.")]
    public GameObject[] HideOnSolve;

    [Header("Artifact pickup text")]
    public string ArtifactName = "Khamun Orb of Sovereignty";

    [Header("Settings")]
    public float ClickRange = 5f;

    // ── Private ───────────────────────────────────────────────────────────────────

    private bool _solved          = false;
    private bool _uiOpen          = false;
    private bool _awaitingPickup  = false;
    private bool _collectPopupVisible = false;

    private int[] _typed;
    private int   _cursor;

    private Camera            _cam;
    private GameObject        _canvasRoot;
    private Image[]           _slotBg;
    private TextMeshProUGUI[] _slotLabels;
    private TextMeshProUGUI   _feedback;
    private GameObject        _collectPopup;

    // Colours
    private static readonly Color ColSlotEmpty  = new Color(0.04f, 0.04f, 0.03f, 1f);
    private static readonly Color ColSlotFilled = new Color(0.07f, 0.06f, 0.02f, 1f);
    private static readonly Color ColSlotCursor = new Color(0.14f, 0.11f, 0.03f, 1f);
    private static readonly Color ColGold       = new Color(0.98f, 0.82f, 0.30f, 1f);
    private static readonly Color ColFeedback   = new Color(0.60f, 0.55f, 0.45f, 1f);
    private static readonly Color ColWrong      = new Color(0.85f, 0.25f, 0.20f, 1f);

    private const string HintMsg  = "Type your code  ·  Backspace to delete  ·  Enter to confirm";
    private const string WrongMsg = "Incorrect year. Study the exhibit again.";

    // Keyboard digit keycodes for quick lookup
    private static readonly KeyCode[] DigitKeys =
    {
        KeyCode.Alpha0, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4,
        KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9
    };
    private static readonly KeyCode[] NumpadKeys =
    {
        KeyCode.Keypad0, KeyCode.Keypad1, KeyCode.Keypad2, KeyCode.Keypad3, KeyCode.Keypad4,
        KeyCode.Keypad5, KeyCode.Keypad6, KeyCode.Keypad7, KeyCode.Keypad8, KeyCode.Keypad9
    };

    // ── Lifecycle ─────────────────────────────────────────────────────────────────

    private void Start()
    {
        _cam   = Camera.main;
        _typed = new int[SecretCode.Length];

        BuildUI();
        SetUIVisible(false);
    }

    private void Update()
    {
        if (_solved)
        {
            if (_collectPopupVisible) return;

            if (_awaitingPickup && Input.GetMouseButtonDown(0))
                TryPickupArtifact();

            return;
        }

        if (!_uiOpen && Input.GetMouseButtonDown(0)) TryClickCase();
        if (_uiOpen)  HandleKeyboard();
    }

    // ── Case click ────────────────────────────────────────────────────────────────

    private void TryClickCase()
    {
        if (_cam == null) return;
        Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, ClickRange)) return;
        if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
            OpenUI();
    }

    // ── Artifact pickup ───────────────────────────────────────────────────────────

    private void TryPickupArtifact()
    {
        if (_cam == null || Artifact == null) return;
        Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, ClickRange)) return;

        if (hit.collider.gameObject == Artifact ||
            hit.collider.transform.IsChildOf(Artifact.transform))
        {
            CollectArtifact();
        }
    }

    /// <summary>Hides the artifact and shows the collected banner briefly.</summary>
    private void CollectArtifact()
    {
        _awaitingPickup      = false;
        _collectPopupVisible = true;

        Artifact.SetActive(false);
        if (_collectPopup != null) _collectPopup.SetActive(true);

        // Restore full FPS control immediately — player can walk while banner fades
        RestorePlayerControl();

        Invoke(nameof(DismissCollectPopup), 2.5f);
    }

    private void DismissCollectPopup()
    {
        _collectPopupVisible = false;
        if (_collectPopup != null) _collectPopup.SetActive(false);
    }

    // ── Player control helpers ────────────────────────────────────────────────────

    private void DisablePlayer()
    {
        if (PlayerController != null) { PlayerController.canMove = false; PlayerController.canLook = false; }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    private void RestorePlayerControl()
    {
        if (PlayerController != null) { PlayerController.canMove = true; PlayerController.canLook = true; }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    /// <summary>Restores full first-person control and unlocks the cursor so the player can click the artifact.</summary>
    private void UnlockCursorForPickup()
    {
        if (PlayerController != null) { PlayerController.canMove = true; PlayerController.canLook = true; }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    // ── Keyboard input ────────────────────────────────────────────────────────────

    /// <summary>Handles all keyboard events while the UI is open.</summary>
    private void HandleKeyboard()
    {
        if (Input.GetKeyDown(KeyCode.Escape))    { CloseUI(); return; }
        if (Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter)) { Confirm(); return; }
        if (Input.GetKeyDown(KeyCode.Backspace))   { DeleteLast(); return; }

        // Number keys — top row and numpad
        for (int d = 0; d <= 9; d++)
        {
            if (Input.GetKeyDown(DigitKeys[d]) || Input.GetKeyDown(NumpadKeys[d]))
            {
                TypeDigit(d);
                return;
            }
        }
    }

    /// <summary>Appends a digit to the next empty slot.</summary>
    private void TypeDigit(int digit)
    {
        if (_cursor >= _typed.Length) return;   // all slots full

        _typed[_cursor] = digit;
        _slotLabels[_cursor].text = digit.ToString();
        _slotBg[_cursor].color = ColSlotFilled;
        _cursor++;

        RefreshCursorHighlight();
        _feedback.text  = HintMsg;
        _feedback.color = ColFeedback;

        // Auto-confirm once all slots are filled
        if (_cursor == _typed.Length) Confirm();
    }

    /// <summary>Removes the last typed digit.</summary>
    private void DeleteLast()
    {
        if (_cursor <= 0) return;

        _cursor--;
        _typed[_cursor]           = 0;
        _slotLabels[_cursor].text = string.Empty;
        _slotBg[_cursor].color    = ColSlotEmpty;

        RefreshCursorHighlight();
        _feedback.text  = HintMsg;
        _feedback.color = ColFeedback;
    }

    /// <summary>Highlights the active (next) slot so the player knows where they are.</summary>
    private void RefreshCursorHighlight()
    {
        for (int i = 0; i < _slotBg.Length; i++)
        {
            if (i == _cursor && _cursor < _typed.Length)
                _slotBg[i].color = ColSlotCursor;
            else if (string.IsNullOrEmpty(_slotLabels[i].text))
                _slotBg[i].color = ColSlotEmpty;
            // filled slots keep ColSlotFilled — already set in TypeDigit
        }
    }

    // ── UI open / close ───────────────────────────────────────────────────────────

    private void OpenUI()
    {
        _uiOpen = true;
        _cursor = 0;
        for (int i = 0; i < _typed.Length; i++)
        {
            _typed[i]           = 0;
            _slotLabels[i].text = string.Empty;
            _slotBg[i].color    = ColSlotEmpty;
        }
        RefreshCursorHighlight();
        _feedback.text  = HintMsg;
        _feedback.color = ColFeedback;

        SetUIVisible(true);
        DisablePlayer();
    }

    private void CloseUI()
    {
        _uiOpen = false;
        SetUIVisible(false);
        RestorePlayerControl();
    }

    private void SetUIVisible(bool show) { if (_canvasRoot != null) _canvasRoot.SetActive(show); }

    // ── Confirm / solve ───────────────────────────────────────────────────────────

    private void Confirm()
    {
        if (_cursor < _typed.Length)
        {
            // Not enough digits typed yet
            _feedback.text  = "Keep typing — " + (_typed.Length - _cursor) + " digit(s) remaining.";
            _feedback.color = ColFeedback;
            return;
        }

        for (int i = 0; i < SecretCode.Length; i++)
        {
            if (_typed[i] != SecretCode[i])
            {
                _feedback.text  = WrongMsg;
                _feedback.color = ColWrong;
                // Flash wrong, reset after a moment
                Invoke(nameof(ResetSlots), 0.8f);
                return;
            }
        }
        Solve();
    }

    /// <summary>Clears all slots after a wrong attempt so the player can try again.</summary>
    private void ResetSlots()
    {
        _cursor = 0;
        for (int i = 0; i < _typed.Length; i++)
        {
            _typed[i]           = 0;
            _slotLabels[i].text = string.Empty;
            _slotBg[i].color    = ColSlotEmpty;
        }
        RefreshCursorHighlight();
        _feedback.text  = HintMsg;
        _feedback.color = ColFeedback;
    }

    private void Solve()
    {
        _solved = true;
        CloseUI();

        // Hide the case shell and everything in HideOnSolve
        foreach (GameObject go in HideOnSolve)
            if (go != null) go.SetActive(false);

        gameObject.SetActive(false);   // hide the case body itself

        // Artifact stays visible — unlock cursor so the player can click it
        _awaitingPickup = true;
        UnlockCursorForPickup();
    }

    // ── UI builder ────────────────────────────────────────────────────────────────

    private void BuildUI()
    {
        // Canvas
        _canvasRoot = new GameObject("MuseumPuzzleCanvas");
        Canvas c = _canvasRoot.AddComponent<Canvas>();
        c.renderMode   = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 200;
        CanvasScaler cs = _canvasRoot.AddComponent<CanvasScaler>();
        cs.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        _canvasRoot.AddComponent<GraphicRaycaster>();

        // Dim overlay
        MakeImg(_canvasRoot.transform, "Dim", Vector2.zero, Vector2.one, new Color(0, 0, 0, 0.65f));

        // Card
        GameObject card = MakeImg(_canvasRoot.transform, "Card",
            new Vector2(0.3f, 0.32f), new Vector2(0.7f, 0.70f),
            new Color(0.08f, 0.07f, 0.05f, 0.97f));

        // Gold header stripe
        GameObject stripe = MakeImg(card.transform, "Stripe",
            new Vector2(0, 0.86f), new Vector2(1, 1),
            new Color(0.72f, 0.56f, 0.15f, 1f));
        Lbl(stripe.transform, "RESTRICTED EXHIBIT  ·  ACCESS REQUIRED",
            24, FontStyles.Bold, Color.black, TextAlignmentOptions.Center,
            Vector2.zero, Vector2.one, new Vector2(8, 4), new Vector2(-8, -4));

        // Sub-header
        Lbl(card.transform, "Display Case  //  Enter Dynasty Birth Year",
            19, FontStyles.Normal, new Color(0.65f, 0.55f, 0.35f, 1f), TextAlignmentOptions.Center,
            new Vector2(0, 0.74f), new Vector2(1, 0.86f), new Vector2(8, 2), new Vector2(-8, -2));

        // ── Digit slots ───────────────────────────────────────────────────────────
        int n = SecretCode.Length;
        _slotBg     = new Image[n];
        _slotLabels = new TextMeshProUGUI[n];

        float padding = 0.03f;
        float slotW   = (1f - padding * (n + 1)) / n;

        for (int i = 0; i < n; i++)
        {
            float x0 = padding + i * (slotW + padding);
            float x1 = x0 + slotW;

            // Slot background
            GameObject slotGO = MakeImg(card.transform, "Slot" + i,
                new Vector2(x0, 0.40f), new Vector2(x1, 0.73f), ColSlotEmpty);
            _slotBg[i] = slotGO.GetComponent<Image>();

            // Gold border strip at bottom of each slot
            MakeImg(slotGO.transform, "Bar",
                new Vector2(0, 0), new Vector2(1, 0.06f),
                new Color(0.6f, 0.47f, 0.12f, 0.8f));

            // Digit text
            _slotLabels[i] = Lbl(slotGO.transform, string.Empty,
                72, FontStyles.Bold, ColGold, TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one, new Vector2(4, 4), new Vector2(-4, -4));
        }

        // Feedback / hint line
        _feedback = Lbl(card.transform, HintMsg,
            16, FontStyles.Normal, ColFeedback, TextAlignmentOptions.Center,
            new Vector2(0, 0.17f), new Vector2(1, 0.40f), new Vector2(10, 4), new Vector2(-10, -4));

        // Key hints at bottom
        Lbl(card.transform, "ENTER  to confirm     BACKSPACE  to delete     ESC  to cancel",
            13, FontStyles.Normal, new Color(0.30f, 0.28f, 0.20f, 1f), TextAlignmentOptions.Center,
            new Vector2(0, 0.01f), new Vector2(1, 0.17f), new Vector2(8, 2), new Vector2(-8, -2));

        // Build the collect popup (auto-dismissing banner)
        _collectPopup = BuildCollectPopup();
        _collectPopup.SetActive(false);
    }

    // ── Collect popup builder ─────────────────────────────────────────────────────

    private GameObject BuildCollectPopup()
    {
        GameObject root = new GameObject("CollectPopupCanvas");
        Canvas cv = root.AddComponent<Canvas>();
        cv.renderMode   = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 300;
        CanvasScaler cs = root.AddComponent<CanvasScaler>();
        cs.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        root.AddComponent<GraphicRaycaster>();

        // Small top-centre banner
        GameObject banner = MakeImg(root.transform, "Banner",
            new Vector2(0.28f, 0.80f), new Vector2(0.72f, 0.93f),
            new Color(0.06f, 0.05f, 0.02f, 0.93f));

        // Gold left accent
        MakeImg(banner.transform, "AccentL",
            new Vector2(0f, 0f), new Vector2(0.012f, 1f),
            new Color(0.85f, 0.65f, 0.1f, 1f));

        // Gold right accent
        MakeImg(banner.transform, "AccentR",
            new Vector2(0.988f, 0f), Vector2.one,
            new Color(0.85f, 0.65f, 0.1f, 1f));

        // "ARTIFACT COLLECTED"
        Lbl(banner.transform, "ARTIFACT COLLECTED",
            22, FontStyles.Bold, ColGold, TextAlignmentOptions.Center,
            new Vector2(0.02f, 0.52f), new Vector2(0.98f, 1f), Vector2.zero, Vector2.zero);

        // Artifact name
        Lbl(banner.transform, ArtifactName,
            16, FontStyles.Italic, new Color(0.88f, 0.80f, 0.55f, 1f), TextAlignmentOptions.Center,
            new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.52f), Vector2.zero, Vector2.zero);

        return root;
    }

    // ── UI helpers ────────────────────────────────────────────────────────────────

    private static GameObject MakeImg(Transform parent, string name,
        Vector2 amin, Vector2 amax, Color col)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = col;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = amin; rt.anchorMax = amax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return go;
    }

    private static TextMeshProUGUI Lbl(Transform parent, string text,
        float size, FontStyles style, Color col, TextAlignmentOptions align,
        Vector2 amin, Vector2 amax, Vector2 omin, Vector2 omax)
    {
        GameObject go = new GameObject("T");
        go.transform.SetParent(parent, false);
        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = size; t.fontStyle = style;
        t.color = col; t.alignment = align;
        t.raycastTarget = false; t.enableWordWrapping = true;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = amin; rt.anchorMax = amax;
        rt.offsetMin = omin; rt.offsetMax = omax;
        return t;
    }
}
