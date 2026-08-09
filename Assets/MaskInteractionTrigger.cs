using UnityEngine;

public class MaskInteractionTrigger : MonoBehaviour
{
    [Header("Gem Reference")]
    [SerializeField] private GameObject gemObject;

    [Header("Interaction Settings")]
    [SerializeField] private bool hideGemOnInteract = true;
    [SerializeField] private bool triggerOnMouseClick = true;

    /// <summary>
    /// Call this method directly from your interaction script or dialogue manager 
    /// right when the cutscene/dialogue sequence starts.
    /// </summary>
    public void OnMaskInteracted()
    {
        if (hideGemOnInteract && gemObject != null)
        {
            gemObject.SetActive(false); // Unchecks the gem's active checkbox in the Inspector
        }
    }

    /// <summary>
    /// Automatically handles mouse clicks on the Mask collider (if using 3D raycasts).
    /// </summary>
    private void OnMouseDown()
    {
        if (triggerOnMouseClick)
        {
            OnMaskInteracted();
        }
    }

    /// <summary>
    /// Optional: Automatically triggers if player steps into a trigger collider around the mask.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnMaskInteracted();
        }
    }
}