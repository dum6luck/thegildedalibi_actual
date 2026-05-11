using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attach to the player camera (same object as ProximityPrompt).
/// Shows a prompt when the player is within range of Julian,
/// and opens the JulianDialogue box when Q is pressed.
///
/// Assign PromptRoot and PromptLabel from the scene Canvas in the Inspector.
/// Edit PromptText to change what the label says.
/// Adjust PromptRange with the slider to set how close the player must be.
/// </summary>
public class JulianProximityPrompt : MonoBehaviour
{
    [Tooltip("The Julian GameObject in the scene.")]
    public Transform Julian;

    [Tooltip("Radius within which the prompt appears and Q can be pressed.")]
    [Range(0.5f, 20f)]
    public float PromptRange = 3f;

    [Header("UI References — assign from scene Canvas")]
    [Tooltip("The root GameObject to show/hide (the pill background and its children).")]
    public GameObject PromptRoot;

    [Tooltip("The TextMeshPro label inside the prompt pill.")]
    public TextMeshProUGUI PromptLabel;

    [Header("Text")]
    [Tooltip("Text shown on the prompt. Change this to relabel the button hint.")]
    public string PromptText = "[ Q ]  Talk";

    private bool isNearJulian;

    private void Start()
    {
        // Apply the editable text to the label once at startup.
        if (PromptLabel != null)
            PromptLabel.text = PromptText;

        SetPromptVisible(false);
    }

    private void Update()
    {
        // Keep label in sync if edited at runtime via Inspector.
        if (PromptLabel != null && PromptLabel.text != PromptText)
            PromptLabel.text = PromptText;

        // Hide prompt while any other screen is open.
        if (InspectionScreen.IsOpen || JulianDialogue.IsOpen)
        {
            SetPromptVisible(false);
            isNearJulian = false;
            return;
        }

        isNearJulian = Julian != null &&
                       Vector3.Distance(transform.position, Julian.position) <= PromptRange;

        SetPromptVisible(isNearJulian);

        if (isNearJulian && Input.GetKeyDown(KeyCode.Q) && JulianDialogue.Instance != null)
        {
            JulianDialogue.Instance.Open();
        }
    }

    private void SetPromptVisible(bool visible)
    {
        if (PromptRoot != null && PromptRoot.activeSelf != visible)
            PromptRoot.SetActive(visible);
    }
}
