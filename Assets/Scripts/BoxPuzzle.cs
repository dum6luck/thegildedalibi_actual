using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Self-contained combination box puzzle.
///
/// The player left-clicks the box to open a number-entry UI.
/// Four arrow buttons let them cycle each digit independently.
/// When the entered code matches the secret code the UI closes,
/// the cursor re-locks, the player regains control, and the lid swings open.
///
/// Assign BoxBody, LidPivot, ClueNote, and FPSController in the Inspector.
/// No other scripts are required.
/// </summary>
public class BoxPuzzle : MonoBehaviour
{
    // ── Inspector fields ────────────────────────────────────────────────────────

    [Header("Secret code (one digit per entry)")]
    public int[] SecretCode = { 1, 9, 4, 4 };

    [Header("Scene references")]
    [Tooltip("The FPS controller on the Player root.")]
    public FPSController PlayerController;

    [Tooltip("The pivot transform that rotates to open the lid.")]
    public Transform LidPivot;

    [Tooltip("The clue note that becomes visible when solved.")]
    public GameObject ClueNote;

    [Header("Lid animation")]
    [Tooltip("How far the lid swings open on its local X axis (degrees).")]
    public float LidOpenAngle = -110f;

    [Tooltip("Seconds the lid takes to open.")]
    public float LidOpenDuration = 0.8f;

    [Header("Interaction")]
    [Tooltip("Max distance from camera to click the box.")]
    public float ClickRange = 4f;

    // ── Private state ───────────────────────────────────────────────────────────

    private bool solved = false;
    private bool uiOpen = false;

    private int[] enteredDigits;

    // Lid animation
    private bool lidAnimating = false;
    private float lidTimer = 0f;
    private Quaternion lidClosedRot;
    private Quaternion lidOpenRot;

    // UI
    private GameObject canvasRoot;
    private TextMeshProUGUI[] digitLabels;
    private TextMeshProUGUI feedbackLabel;
    private Camera playerCamera;

    // ── Constants ───────────────────────────────────────────────────────────────

    private const string WrongFeedback = "Wrong code. Try again.";
    private const string HintText      = "Click  ▲  /  ▼  to change each digit,  then  CONFIRM";

    // ── Lifecycle ───────────────────────────────────────────────────────────────

    private void Start()
    {
        playerCamera = Camera.main;

        enteredDigits = new int[SecretCode.Length];

        if (LidPivot != null)
        {
            lidClosedRot = LidPivot.localRotation;
            lidOpenRot   = Quaternion.Euler(LidOpenAngle, 0f, 0f) * lidClosedRot;
        }

        if (ClueNote != null)
            ClueNote.SetActive(false);

        BuildUI();
        CloseUI();
    }

    private void Update()
    {
        AnimateLid();

        if (solved) return;

        // Detect click on the box when UI is closed
        if (!uiOpen && Input.GetMouseButtonDown(0))
        {
            TryOpenUI();
        }

        // ESC closes UI
        if (uiOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseUI();
        }
    }

    // ── Box click detection ─────────────────────────────────────────────────────

    private void TryOpenUI()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, ClickRange)) return;

        // Did the player click this box (or any child)?
        if (hit.collider.gameObject == gameObject ||
            hit.collider.transform.IsChildOf(transform))
        {
            OpenUI();
        }
    }

    // ── UI open / close ─────────────────────────────────────────────────────────

    private void OpenUI()
    {
        uiOpen = true;
        canvasRoot.SetActive(true);

        // Reset entered digits and labels
        for (int i = 0; i < enteredDigits.Length; i++)
        {
            enteredDigits[i] = 0;
            digitLabels[i].text = "0";
        }

        if (feedbackLabel != null) feedbackLabel.text = HintText;

        // Release cursor so buttons are clickable
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        // Freeze player
        if (PlayerController != null)
        {
            PlayerController.canMove = false;
            PlayerController.canLook = false;
        }
    }

    private void CloseUI()
    {
        uiOpen = false;
        if (canvasRoot != null) canvasRoot.SetActive(false);

        // Re-lock cursor and restore player
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        if (PlayerController != null)
        {
            PlayerController.canMove = true;
            PlayerController.canLook = true;
        }
    }

    // ── Digit button callbacks ──────────────────────────────────────────────────

    /// <summary>Called by the Up button for digit at index.</summary>
    private void IncrementDigit(int index)
    {
        enteredDigits[index] = (enteredDigits[index] + 1) % 10;
        digitLabels[index].text = enteredDigits[index].ToString();
        if (feedbackLabel != null) feedbackLabel.text = HintText;
    }

    /// <summary>Called by the Down button for digit at index.</summary>
    private void DecrementDigit(int index)
    {
        enteredDigits[index] = (enteredDigits[index] + 9) % 10;  // +9 mod 10 = minus 1 with wrap
        digitLabels[index].text = enteredDigits[index].ToString();
        if (feedbackLabel != null) feedbackLabel.text = HintText;
    }

    // ── Confirm / check ─────────────────────────────────────────────────────────

    private void CheckCode()
    {
        bool correct = true;
        for (int i = 0; i < SecretCode.Length; i++)
        {
            if (enteredDigits[i] != SecretCode[i]) { correct = false; break; }
        }

        if (correct)
        {
            Solve();
        }
        else
        {
            if (feedbackLabel != null) feedbackLabel.text = WrongFeedback;
        }
    }

    private void Solve()
    {
        solved = true;
        CloseUI();

        // Start lid animation
        lidAnimating = true;
        lidTimer = 0f;

        if (ClueNote != null)
            ClueNote.SetActive(true);
    }

    // ── Lid animation ───────────────────────────────────────────────────────────

    private void AnimateLid()
    {
        if (!lidAnimating || LidPivot == null) return;

        lidTimer += Time.deltaTime;
        float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(lidTimer / LidOpenDuration));
        LidPivot.localRotation = Quaternion.Slerp(lidClosedRot, lidOpenRot, t);

        if (lidTimer >= LidOpenDuration)
            lidAnimating = false;
    }

    // ── UI construction ─────────────────────────────────────────────────────────

    /// <summary>Builds the entire code-entry UI at runtime — no prefabs needed.</summary>
    private void BuildUI()
    {
        // Root canvas
        canvasRoot = new GameObject("BoxPuzzleCanvas");
        Canvas canvas = canvasRoot.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        CanvasScaler scaler = canvasRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasRoot.AddComponent<GraphicRaycaster>();

        // Dark semi-transparent overlay behind the panel
        GameObject bg = new GameObject("Dimmer");
        bg.transform.SetParent(canvasRoot.transform, false);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0f, 0f, 0f, 0.7f);
        StretchRect(bg.GetComponent<RectTransform>());

        // Centre card
        GameObject card = new GameObject("Card");
        card.transform.SetParent(canvasRoot.transform, false);
        Image cardImg = card.AddComponent<Image>();
        cardImg.color = new Color(0.1f, 0.08f, 0.06f, 0.97f);
        RectTransform cardRT = card.GetComponent<RectTransform>();
        cardRT.anchorMin = new Vector2(0.3f, 0.3f);
        cardRT.anchorMax = new Vector2(0.7f, 0.72f);
        cardRT.offsetMin = cardRT.offsetMax = Vector2.zero;

        // Title
        MakeText(card.transform, "COMBINATION LOCK", 32, FontStyles.Bold,
            new Color(1f, 0.82f, 0.3f, 1f), TextAlignmentOptions.Center,
            new Vector2(0f, 0.75f), new Vector2(1f, 1f),
            new Vector2(15f, 5f), new Vector2(-15f, -5f));

        // Digit row
        int count = SecretCode.Length;
        digitLabels = new TextMeshProUGUI[count];

        float slotWidth = 1f / count;

        for (int i = 0; i < count; i++)
        {
            float xMin = i * slotWidth;
            float xMax = (i + 1) * slotWidth;

            // Up button ▲
            GameObject upBtn = MakeButton(card.transform, "▲",
                new Vector2(xMin + 0.05f, 0.62f), new Vector2(xMax - 0.05f, 0.74f));
            int captured = i;
            upBtn.GetComponent<Button>().onClick.AddListener(() => IncrementDigit(captured));

            // Digit display
            GameObject digitGO = new GameObject($"Digit{i}");
            digitGO.transform.SetParent(card.transform, false);
            Image digitBg = digitGO.AddComponent<Image>();
            digitBg.color = new Color(0.06f, 0.06f, 0.06f, 1f);
            RectTransform digitRT = digitGO.GetComponent<RectTransform>();
            SetAnchors(digitRT,
                new Vector2(xMin + 0.07f, 0.36f), new Vector2(xMax - 0.07f, 0.62f),
                Vector2.zero, Vector2.zero);

            digitLabels[i] = MakeText(digitGO.transform, "0", 56, FontStyles.Bold,
                Color.white, TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one, new Vector2(4f, 4f), new Vector2(-4f, -4f));

            // Down button ▼
            GameObject downBtn = MakeButton(card.transform, "▼",
                new Vector2(xMin + 0.05f, 0.24f), new Vector2(xMax - 0.05f, 0.36f));
            downBtn.GetComponent<Button>().onClick.AddListener(() => DecrementDigit(captured));
        }

        // Feedback label (hint / error)
        feedbackLabel = MakeText(card.transform, HintText, 18, FontStyles.Normal,
            new Color(0.75f, 0.75f, 0.72f, 1f), TextAlignmentOptions.Center,
            new Vector2(0f, 0.1f), new Vector2(1f, 0.24f),
            new Vector2(15f, 4f), new Vector2(-15f, -4f));

        // Confirm button
        GameObject confirmBtn = MakeButton(card.transform, "CONFIRM",
            new Vector2(0.15f, 0.01f), new Vector2(0.85f, 0.1f),
            new Color(0.2f, 0.55f, 0.2f, 1f), 26, FontStyles.Bold);
        confirmBtn.GetComponent<Button>().onClick.AddListener(CheckCode);
    }

    // ── UI helpers ──────────────────────────────────────────────────────────────

    private static void StretchRect(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    private static void SetAnchors(RectTransform rt,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax)
    {
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
    }

    private static TextMeshProUGUI MakeText(Transform parent, string text,
        float size, FontStyles style, Color color, TextAlignmentOptions align,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = size;
        tmp.fontStyle = style;
        tmp.color     = color;
        tmp.alignment = align;
        tmp.raycastTarget = false;
        SetAnchors(go.GetComponent<RectTransform>(), anchorMin, anchorMax, offsetMin, offsetMax);
        return tmp;
    }

    private static GameObject MakeButton(Transform parent, string label,
        Vector2 anchorMin, Vector2 anchorMax,
        Color? bgColor = null, float fontSize = 24, FontStyles fontStyle = FontStyles.Bold)
    {
        GameObject go = new GameObject("Button_" + label);
        go.transform.SetParent(parent, false);

        Image img = go.AddComponent<Image>();
        img.color = bgColor ?? new Color(0.25f, 0.2f, 0.15f, 1f);

        Button btn = go.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor      = img.color;
        cb.highlightedColor = img.color * 1.25f;
        cb.pressedColor     = img.color * 0.8f;
        btn.colors = cb;

        SetAnchors(go.GetComponent<RectTransform>(), anchorMin, anchorMax, Vector2.zero, Vector2.zero);

        // Label
        GameObject textGO = new GameObject("Label");
        textGO.transform.SetParent(go.transform, false);
        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = fontSize;
        tmp.fontStyle = fontStyle;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        StretchRect(textGO.GetComponent<RectTransform>());

        return go;
    }
}
