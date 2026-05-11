using UnityEngine;
using TMPro;

/// <summary>
/// Manages the three-dial combination lock puzzle.
///
/// The player walks up to the box, looks at a dial, and presses F to cycle it.
/// When all three dials match the SecretCode the lid animates open, the
/// InspectableClueObject is activated, and the lock is permanently disabled.
///
/// Interaction range and the F-key prompt are handled internally; no
/// separate ProximityPrompt is needed for the dials.
/// </summary>
public class CombinationLockBox : MonoBehaviour
{
    [Header("Combination")]
    [Tooltip("Three-digit code the player must dial in (e.g. {1, 9, 4}).")]
    public int[] SecretCode = { 1, 9, 4 };

    [Header("Dials")]
    public DialWheel Dial0;
    public DialWheel Dial1;
    public DialWheel Dial2;

    [Header("Lid")]
    [Tooltip("The lid GameObject that rotates open when solved.")]
    public Transform Lid;

    [Tooltip("Local X-axis rotation the lid opens to (degrees).")]
    public float LidOpenAngle = -110f;

    [Tooltip("Seconds the lid takes to swing open.")]
    public float LidOpenDuration = 0.6f;

    [Header("Reward")]
    [Tooltip("The clue note that becomes visible once the box is open.")]
    public GameObject ClueNoteObject;

    [Header("Interaction")]
    [Tooltip("How far away the player can interact with dials.")]
    public float InteractRange = 3f;

    [Tooltip("Prompt shown when looking at a dial.")]
    public string PromptText = "[ F ]  Turn dial";

    // ── Runtime state ──────────────────────────────────────────────────────────

    private bool solved = false;
    private bool lidOpening = false;
    private float lidTimer = 0f;
    private Quaternion lidClosedRot;
    private Quaternion lidOpenRot;

    private Camera playerCamera;
    private DialWheel hoveredDial;

    private GameObject promptCanvasGO;
    private TextMeshProUGUI promptLabel;
    private GameObject promptRoot;

    // ── Lifecycle ──────────────────────────────────────────────────────────────

    private void Start()
    {
        playerCamera = Camera.main;
        if (playerCamera == null)
            playerCamera = FindObjectOfType<Camera>();

        if (Lid != null)
        {
            lidClosedRot = Lid.localRotation;
            lidOpenRot   = Quaternion.Euler(LidOpenAngle, 0f, 0f) * lidClosedRot;
        }

        if (ClueNoteObject != null)
            ClueNoteObject.SetActive(false);

        BuildPromptUI();
    }

    private void Update()
    {
        if (solved)
        {
            AnimateLid();
            SetPromptVisible(false);
            return;
        }

        DetectHoveredDial();

        if (hoveredDial != null)
        {
            SetPromptVisible(true);
            if (Input.GetKeyDown(KeyCode.F))
            {
                hoveredDial.CycleUp();
                CheckSolution();
            }
        }
        else
        {
            SetPromptVisible(false);
        }
    }

    // ── Puzzle logic ───────────────────────────────────────────────────────────

    private void DetectHoveredDial()
    {
        if (playerCamera == null || InspectionScreen.IsOpen || JulianDialogue.IsOpen)
        {
            hoveredDial = null;
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, InteractRange))
        {
            DialWheel dial = hit.collider.GetComponentInParent<DialWheel>();
            hoveredDial = dial;
        }
        else
        {
            hoveredDial = null;
        }
    }

    private void CheckSolution()
    {
        if (SecretCode.Length < 3) return;
        if (Dial0 == null || Dial1 == null || Dial2 == null) return;

        bool correct = Dial0.CurrentDigit == SecretCode[0]
                    && Dial1.CurrentDigit == SecretCode[1]
                    && Dial2.CurrentDigit == SecretCode[2];

        if (!correct) return;

        solved     = true;
        lidOpening = true;
        lidTimer   = 0f;

        if (ClueNoteObject != null)
            ClueNoteObject.SetActive(true);
    }

    private void AnimateLid()
    {
        if (!lidOpening || Lid == null) return;

        lidTimer += Time.deltaTime;
        float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(lidTimer / LidOpenDuration));
        Lid.localRotation = Quaternion.Slerp(lidClosedRot, lidOpenRot, t);

        if (lidTimer >= LidOpenDuration)
            lidOpening = false;
    }

    // ── Prompt UI ──────────────────────────────────────────────────────────────

    private void SetPromptVisible(bool visible)
    {
        if (promptRoot != null && promptRoot.activeSelf != visible)
            promptRoot.SetActive(visible);

        if (promptLabel != null && hoveredDial != null)
            promptLabel.text = PromptText;
    }

    private void BuildPromptUI()
    {
        promptCanvasGO = new GameObject("LockBoxPromptCanvas");
        var canvas = promptCanvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 60;

        var scaler = promptCanvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode         = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        promptCanvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        promptRoot = new GameObject("PromptRoot");
        promptRoot.transform.SetParent(promptCanvasGO.transform, false);

        GameObject pill = new GameObject("Pill");
        pill.transform.SetParent(promptRoot.transform, false);
        var pillImg = pill.AddComponent<UnityEngine.UI.Image>();
        pillImg.color = new Color(0f, 0f, 0f, 0.72f);

        var pillRT = pill.GetComponent<RectTransform>();
        pillRT.anchorMin = new Vector2(0.35f, 0.09f);
        pillRT.anchorMax = new Vector2(0.65f, 0.16f);
        pillRT.offsetMin = pillRT.offsetMax = Vector2.zero;

        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(pill.transform, false);
        promptLabel = textGO.AddComponent<TextMeshProUGUI>();
        promptLabel.text      = PromptText;
        promptLabel.fontSize   = 30;
        promptLabel.fontStyle  = FontStyles.Bold;
        promptLabel.color      = Color.white;
        promptLabel.alignment  = TextAlignmentOptions.Center;
        promptLabel.raycastTarget = false;

        var textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero; textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(15f, 5f); textRT.offsetMax = new Vector2(-15f, -5f);

        promptRoot.SetActive(false);
    }
}
