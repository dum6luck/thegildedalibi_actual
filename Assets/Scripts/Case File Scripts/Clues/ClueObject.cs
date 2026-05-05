using UnityEngine;

public class ClueObject : MonoBehaviour
{
    [Header("Data")]
    public Clue clueAsset; // The ScriptableObject asset for this specific clue

    [Header("Visual Feedback")]
    public GameObject hoverOutline; // The "Shape" or highlight object

    private void Start()
    {
        // Hide the highlight/shape until we hover over it
        if (hoverOutline != null) hoverOutline.SetActive(false);
    }

    public void Interact()
    {
        // 1. Save the clue to your global manager for the Case File UI
        if (ClueManager.Instance != null)
        {
            ClueManager.Instance.AddClue(clueAsset);
        }

        // 2. Trigger the Detective's dialogue from your existing Talking_Manager
        Talking_Manager manager = FindObjectOfType<Talking_Manager>();
        if (manager != null)
        {
            manager.gameObject.SetActive(true);
            manager.StartDialogueSequence(false);
        }
    }

    // These handle the "Hover Shape" logic you requested
    private void OnMouseEnter()
    {
        if (hoverOutline != null) hoverOutline.SetActive(true);
    }

    private void OnMouseExit()
    {
        if (hoverOutline != null) hoverOutline.SetActive(false);
    }
}