using UnityEngine;
using TMPro;

public class ClueInteraction : MonoBehaviour
{
    public float interactRange = 4f;
    public GameObject magnifyingGlassUI;
    public TextMeshProUGUI interactPromptE;

    void Update()
    {
        // Only works if magnifying glass is out (Press I first!)
        if (magnifyingGlassUI != null && !magnifyingGlassUI.activeSelf)
        {
            interactPromptE.gameObject.SetActive(false);
            return;
        }

        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        // "DefaultRaycastLayers" tells it to hit everything (since you aren't using a LayerMask)
        if (Physics.Raycast(ray, out hit, interactRange))
        {
            // Check if the object we hit has the "Clue" tag
            if (hit.collider.CompareTag("Clue"))
            {
                if (hit.collider.TryGetComponent(out ClueObject clue))
                {
                    interactPromptE.text = $"Press E to Inspect {clue.clueAsset.clueName}";
                    interactPromptE.gameObject.SetActive(true);

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        clue.Interact();
                    }
                }
            }
            else
            {
                interactPromptE.gameObject.SetActive(false);
            }
        }
    }
}