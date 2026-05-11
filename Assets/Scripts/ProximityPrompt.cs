using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attach to the player camera.
/// Scans nearby InspectableObjects and shows a prompt when in range.
/// Pressing Q opens InspectionScreen.
///
/// Assign PromptRoot and PromptLabel from the scene Canvas in the Inspector.
/// Edit PromptText to change what the label says.
/// </summary>
public class ProximityPrompt : MonoBehaviour
{
    [Tooltip("Radius within which the prompt appears.")]
    [Range(0.5f, 20f)]
    public float PromptRange = 3f;

    [Header("UI References — assign from scene Canvas")]
    [Tooltip("The root GameObject to show/hide (the pill background and its children).")]
    public GameObject PromptRoot;

    [Tooltip("The TextMeshPro label inside the prompt pill.")]
    public TextMeshProUGUI PromptLabel;

    [Header("Text")]
    [Tooltip("Text shown on the prompt. Change this to relabel the button hint.")]
    public string PromptText = "[ Q ]  Inspect";

    private InspectableObject nearestTarget;

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

        // Hide prompt while the inspection screen is open.
        if (InspectionScreen.IsOpen)
        {
            SetPromptVisible(false);
            nearestTarget = null;
            return;
        }

        nearestTarget = FindNearest();
        SetPromptVisible(nearestTarget != null);

        if (nearestTarget != null && Input.GetKeyDown(KeyCode.Q) && InspectionScreen.Instance != null)
        {
            InspectionScreen.Instance.Open(nearestTarget);
        }
    }

    /// <summary>Returns the closest InspectableObject within PromptRange, or null.</summary>
    private InspectableObject FindNearest()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, PromptRange);
        InspectableObject closest = null;
        float bestDist = float.MaxValue;

        foreach (Collider col in hits)
        {
            InspectableObject obj = col.GetComponentInParent<InspectableObject>();
            if (obj == null) continue;

            float dist = Vector3.Distance(transform.position, obj.transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                closest = obj;
            }
        }

        return closest;
    }

    private void SetPromptVisible(bool visible)
    {
        if (PromptRoot != null && PromptRoot.activeSelf != visible)
            PromptRoot.SetActive(visible);
    }
}
