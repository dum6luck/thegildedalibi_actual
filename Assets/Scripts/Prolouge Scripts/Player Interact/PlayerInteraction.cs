using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Settings")]
    public float interactionDistance = 4f;
    public string npcTag = "NPC";

    [Header("UI References")]
    public GameObject interactPrompt;
    public Talking_Manager talkingManager;
    public FPSController fpsController;

    [Header("Progression")]
    public MingleTracker mingleTracker;

    void Update()
    {
        // Don't interact if the dialogue box is already open
        if (talkingManager != null && talkingManager.gameObject.activeSelf)
        {
            if (interactPrompt != null) interactPrompt.SetActive(false);
            return;
        }

        CheckForInteractables();
    }

    void CheckForInteractables()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        // Visual debug line for Scene view
        Debug.DrawRay(transform.position, transform.forward * interactionDistance, Color.yellow);

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            // CHECK FOR NPCs
            if (hit.collider.CompareTag(npcTag))
            {
                if (interactPrompt != null) interactPrompt.SetActive(true);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    NPCData data = hit.collider.GetComponent<NPCData>();
                    if (data != null) StartConversation(data);
                }
            }
            else
            {
                if (interactPrompt != null) interactPrompt.SetActive(false);
            }
        }
        else
        {
            if (interactPrompt != null) interactPrompt.SetActive(false);
        }
    }

    void StartConversation(NPCData data)
    {
        if (interactPrompt != null) interactPrompt.SetActive(false);

        // Freeze player movement
        fpsController.canMove = false;
        fpsController.canLook = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Use the Interact method from NPCData to handle line loading and the MingleTracker
        data.Interact(talkingManager);
    }
}