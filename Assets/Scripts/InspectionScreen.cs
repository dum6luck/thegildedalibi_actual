using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Resident Evil-style item inspection screen.
///
/// When Open() is called the object is teleported to an isolated inspection
/// stage (far below the scene), a dedicated camera takes over rendering with
/// a dark solid-colour background, and a UI overlay shows the object name
/// and controls. The player rotates the object with WASD via ObjectRotator.
///
/// While open the cursor is unlocked so the player can left-click ArtifactClue
/// child objects to reveal their clue text in a centred popup.
/// Escape dismisses the popup first, then exits inspection on a second press.
///
/// Setup: create an empty "InspectionManager" in the scene, attach this
/// component + ObjectRotator, and assign the public fields in the Inspector.
/// </summary>
public class InspectionScreen : MonoBehaviour
{
    public static InspectionScreen Instance { get; private set; }

    /// <summary>True while the inspection screen is open.</summary>
    public static bool IsOpen { get; private set; }

    [Header("Scene References")]
    [Tooltip("The dedicated camera used only during inspection.")]
    public Camera InspectionCamera;

    [Tooltip("World position where the object is placed during inspection.")]
    public Transform InspectionPoint;

    [Tooltip("ObjectRotator that handles WASD input during inspection.")]
    public ObjectRotator ObjectRotator;

    [Header("Timing")]
    [Tooltip("Seconds the object takes to smoothly fly back to its pedestal on close.")]
    public float ReturnDuration = 0.5f;

    private FPSController fpsController;

    private InspectableObject currentObject;
    private Rigidbody currentRigidbody;
    private Vector3 storedPosition;
    private Quaternion storedRotation;
    private bool storedKinematic;

    // Main overlay
    private GameObject overlayRoot;
    private TextMeshProUGUI nameLabel;

    // Clue popup
    private GameObject cluePopupRoot;
    private TextMeshProUGUI clueBodyLabel;
    private bool cluePopupShowing;

    private void Awake()
    {
        Instance = this;
        InspectionCamera.enabled = false;
        BuildUI();
        SetOverlayActive(false);
    }

    private void Start()
    {
        fpsController = FindObjectOfType<FPSController>();
        InspectionCamera.transform.LookAt(InspectionPoint);
    }

    private void OnEnable()  => ArtifactClue.OnClueDiscovered += ShowCluePopup;
    private void OnDisable() => ArtifactClue.OnClueDiscovered -= ShowCluePopup;

    private void Update()
    {
        if (!IsOpen) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (cluePopupShowing) HideCluePopup();
            else Close();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (cluePopupShowing) HideCluePopup();
            else TryClickClue();
        }
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>Opens the inspection screen for the given object.</summary>
    public void Open(InspectableObject obj)
    {
        if (IsOpen) return;
        IsOpen = true;

        currentObject = obj;
        currentRigidbody = obj.GetComponent<Rigidbody>();

        storedPosition = obj.transform.position;
        storedRotation = obj.transform.rotation;

        if (currentRigidbody != null)
        {
            storedKinematic = currentRigidbody.isKinematic;
            currentRigidbody.isKinematic = true;
            currentRigidbody.velocity = Vector3.zero;
            currentRigidbody.angularVelocity = Vector3.zero;
        }

        obj.transform.position = InspectionPoint.position;
        obj.transform.rotation = Quaternion.identity;

        if (fpsController != null)
        {
            fpsController.canMove = false;
            fpsController.canLook = false;
        }

        // Unlock cursor so the player can aim and click clues.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        InspectionCamera.enabled = true;
        if (nameLabel != null) nameLabel.text = obj.DisplayName.ToUpper();
        SetOverlayActive(true);

        ObjectRotator.StartRotating(obj.transform);
    }

    /// <summary>Closes the screen and returns the object to its original position.</summary>
    public void Close()
    {
        if (!IsOpen) return;

        HideCluePopup();
        ObjectRotator.StopRotating();
        InspectionCamera.enabled = false;
        SetOverlayActive(false);

        StartCoroutine(ReturnCoroutine());
    }

    // -------------------------------------------------------------------------
    // Clue interaction
    // -------------------------------------------------------------------------

    /// <summary>
    /// Casts a ray from the InspectionCamera through the current mouse position.
    /// If it hits an ArtifactClue collider, calls Reveal() on it.
    /// </summary>
    private void TryClickClue()
    {
        Ray ray = InspectionCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, InspectionCamera.farClipPlane)) return;

        ArtifactClue clue = hit.collider.GetComponent<ArtifactClue>();
        clue?.Reveal();
    }

    private void ShowCluePopup(string text)
    {
        if (clueBodyLabel != null) clueBodyLabel.text = text;
        cluePopupRoot?.SetActive(true);
        cluePopupShowing = true;
    }

    private void HideCluePopup()
    {
        cluePopupRoot?.SetActive(false);
        cluePopupShowing = false;
    }

    // -------------------------------------------------------------------------
    // Internals
    // -------------------------------------------------------------------------

    private IEnumerator ReturnCoroutine()
    {
        InspectableObject obj = currentObject;
        Vector3 fromPos = obj.transform.position;
        Quaternion fromRot = obj.transform.rotation;
        float elapsed = 0f;

        while (elapsed < ReturnDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / ReturnDuration));
            obj.transform.position = Vector3.Lerp(fromPos, storedPosition, t);
            obj.transform.rotation = Quaternion.Slerp(fromRot, storedRotation, t);
            yield return null;
        }

        obj.transform.SetPositionAndRotation(storedPosition, storedRotation);

        if (currentRigidbody != null)
            currentRigidbody.isKinematic = storedKinematic;

        if (fpsController != null)
        {
            fpsController.canMove = true;
            fpsController.canLook = true;
        }

        // Re-lock cursor for walking mode.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentObject = null;
        currentRigidbody = null;
        IsOpen = false;
    }

    private void SetOverlayActive(bool active) => overlayRoot?.SetActive(active);

    // -------------------------------------------------------------------------
    // UI construction
    // -------------------------------------------------------------------------

    /// <summary>Builds the full inspection UI at runtime — no prefabs required.</summary>
    private void BuildUI()
    {
        GameObject canvasGO = new GameObject("InspectionCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // ---- Main overlay (top + bottom bars) ----
        overlayRoot = new GameObject("InspectionOverlay");
        overlayRoot.transform.SetParent(canvasGO.transform, false);
        RectTransform rootRT = overlayRoot.AddComponent<RectTransform>();
        rootRT.anchorMin = Vector2.zero;
        rootRT.anchorMax = Vector2.one;
        rootRT.offsetMin = rootRT.offsetMax = Vector2.zero;

        GameObject topBar = MakePanel(overlayRoot.transform,
            new Vector2(0f, 0.88f), new Vector2(1f, 1f),
            new Color(0f, 0f, 0f, 0.82f));
        nameLabel = MakeLabel(topBar.transform, "", 36, FontStyles.Bold,
            new Color(1f, 0.88f, 0.5f, 1f), TextAlignmentOptions.Center);

        GameObject bottomBar = MakePanel(overlayRoot.transform,
            new Vector2(0f, 0f), new Vector2(1f, 0.1f),
            new Color(0f, 0f, 0f, 0.82f));
        MakeLabel(bottomBar.transform,
            "W / S  ·  Tilt          A / D  ·  Spin          LMB  ·  Click clue          ESC  ·  Exit",
            20, FontStyles.Normal,
            new Color(0.78f, 0.78f, 0.78f, 1f), TextAlignmentOptions.Center);

        // ---- Clue popup (centre card, above overlay) ----
        cluePopupRoot = new GameObject("CluePopup");
        cluePopupRoot.transform.SetParent(canvasGO.transform, false);
        Image popupBg = cluePopupRoot.AddComponent<Image>();
        popupBg.color = new Color(0.04f, 0.04f, 0.04f, 0.96f);
        RectTransform popupRT = cluePopupRoot.GetComponent<RectTransform>();
        popupRT.anchorMin = new Vector2(0.2f, 0.3f);
        popupRT.anchorMax = new Vector2(0.8f, 0.72f);
        popupRT.offsetMin = popupRT.offsetMax = Vector2.zero;

        // Title strip
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(cluePopupRoot.transform, false);
        TextMeshProUGUI titleLabel = titleGO.AddComponent<TextMeshProUGUI>();
        titleLabel.text = "CLUE DISCOVERED";
        titleLabel.fontSize = 20;
        titleLabel.fontStyle = FontStyles.Bold | FontStyles.SmallCaps;
        titleLabel.color = new Color(1f, 0.76f, 0.2f, 1f);
        titleLabel.alignment = TextAlignmentOptions.Center;
        RectTransform titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0f, 0.74f);
        titleRT.anchorMax = Vector2.one;
        titleRT.offsetMin = new Vector2(20f, 0f);
        titleRT.offsetMax = new Vector2(-20f, -10f);

        // Clue body text
        GameObject bodyGO = new GameObject("ClueBody");
        bodyGO.transform.SetParent(cluePopupRoot.transform, false);
        clueBodyLabel = bodyGO.AddComponent<TextMeshProUGUI>();
        clueBodyLabel.fontSize = 26;
        clueBodyLabel.fontStyle = FontStyles.Italic;
        clueBodyLabel.color = new Color(0.93f, 0.93f, 0.88f, 1f);
        clueBodyLabel.alignment = TextAlignmentOptions.Center;
        RectTransform bodyRT = bodyGO.GetComponent<RectTransform>();
        bodyRT.anchorMin = new Vector2(0f, 0.26f);
        bodyRT.anchorMax = new Vector2(1f, 0.74f);
        bodyRT.offsetMin = new Vector2(30f, 0f);
        bodyRT.offsetMax = new Vector2(-30f, 0f);

        // Dismiss hint
        GameObject dismissGO = new GameObject("DismissHint");
        dismissGO.transform.SetParent(cluePopupRoot.transform, false);
        TextMeshProUGUI dismissLabel = dismissGO.AddComponent<TextMeshProUGUI>();
        dismissLabel.text = "[ Click or ESC to dismiss ]";
        dismissLabel.fontSize = 16;
        dismissLabel.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        dismissLabel.alignment = TextAlignmentOptions.Center;
        RectTransform dismissRT = dismissGO.GetComponent<RectTransform>();
        dismissRT.anchorMin = Vector2.zero;
        dismissRT.anchorMax = new Vector2(1f, 0.26f);
        dismissRT.offsetMin = new Vector2(20f, 8f);
        dismissRT.offsetMax = new Vector2(-20f, 0f);

        cluePopupRoot.SetActive(false);
    }

    private static GameObject MakePanel(Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        GameObject go = new GameObject("Panel");
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = color;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return go;
    }

    private static TextMeshProUGUI MakeLabel(Transform parent, string text, float size,
        FontStyles style, Color color, TextAlignmentOptions align)
    {
        GameObject go = new GameObject("Label");
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = align;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(20f, 8f);
        rt.offsetMax = new Vector2(-20f, -8f);
        return tmp;
    }
}
