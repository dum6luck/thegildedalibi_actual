using UnityEngine;

public class MaskInteractionTrigger : MonoBehaviour
{
    [Header("Gem Reference")]
    [SerializeField] private GameObject gemObject;

    [Header("Inventory Settings")]
    [SerializeField] private string itemID = "Gem";

    [Header("Interaction Settings")]
    [SerializeField] private bool hideGemOnInteract = true;
    [SerializeField] private bool triggerOnMouseClick = true;

    /// <summary>
    /// Call this method directly from your interaction script or dialogue manager 
    /// right when the cutscene/dialogue sequence starts.
    /// </summary>
    public void OnMaskInteracted()
    {
        // Add gem to UserInventory
        if (UserInventory.Instance != null)
        {
            UserInventory.Instance.AddItem(itemID);
        }
        else
        {
            Debug.LogWarning("UserInventory Instance not found in scene!");
        }

        // Hide gem object in scene
        if (hideGemOnInteract && gemObject != null)
        {
            gemObject.SetActive(false);
        }
    }

    private void OnMouseDown()
    {
        if (triggerOnMouseClick)
        {
            OnMaskInteracted();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnMaskInteracted();
        }
    }
}