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
public class GrabbableProximityPrompt : MonoBehaviour
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
    public string PromptText = "[ Q ]  Grab";

    private GrabbableObject nearestTarget;
    
    private bool isGrabbed = false;

    private void Start()
    {
        // Apply the editable text to the label once at startup.
        if (PromptLabel != null)
            PromptLabel.text = PromptText;

        SetPromptVisible(false);
    }

    private void Update()
    {
        nearestTarget = FindNearest();

        // Hide prompt while the inspection screen is open.
        if (isGrabbed)
        {
            SetPromptVisible(false);
            nearestTarget.transform.position = transform.parent.TransformPoint(new Vector3(0.75f, 0.75f, 1f));
            return;
        }

        SetPromptVisible(nearestTarget != null);
        if (nearestTarget != null && Input.GetKeyDown(KeyCode.Q))
        {
            isGrabbed = true;
        }
    }

    /// <summary>Returns the closest GrabbableObject within PromptRange, or null.</summary>
    private GrabbableObject FindNearest()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, PromptRange);
        GrabbableObject closest = null;
        float bestDist = float.MaxValue;

        foreach (Collider col in hits)
        {
            GrabbableObject obj = col.GetComponentInParent<GrabbableObject>();
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
