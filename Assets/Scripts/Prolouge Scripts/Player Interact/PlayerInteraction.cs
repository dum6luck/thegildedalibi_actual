using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Settings")]
    public float interactionDistance = 4f;
    public string npcTag = "NPC";
    public string doorTag = "Door"; // NEW: Make sure to create this tag in Unity!

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
            // 1. CHECK FOR NPCs
            if (hit.collider.CompareTag(npcTag))
            {
                if (interactPrompt != null) interactPrompt.SetActive(true);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    NPCData data = hit.collider.GetComponent<NPCData>();
                    if (data != null) StartConversation(data);
                }
            }
            // 2. CHECK FOR DOORS
            else if (hit.collider.CompareTag(doorTag))
            {
                if (interactPrompt != null) interactPrompt.SetActive(true);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    BathroomTransition door = hit.collider.GetComponent<BathroomTransition>();
                    if (door != null) door.Interact();
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

        fpsController.canMove = false;
        fpsController.canLook = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        talkingManager.dialogueLines.Clear();

        if (!data.hasTalked)
        {
            foreach (var line in data.conversation)
            {
                Talking_Manager.DialogueLine newLine = new Talking_Manager.DialogueLine();
                newLine.characterName = line.characterName;
                newLine.sentence = line.sentence;
                newLine.isItalic = line.isItalic;
                talkingManager.dialogueLines.Add(newLine);
            }

            data.hasTalked = true;
            talkingManager.gameObject.SetActive(true);
            talkingManager.StartDialogueSequence(true);
        }
        else
        {
            Talking_Manager.DialogueLine newLine = new Talking_Manager.DialogueLine();
            newLine.characterName = data.repeatLine.characterName;
            newLine.sentence = data.repeatLine.sentence;
            newLine.isItalic = data.repeatLine.isItalic;
            talkingManager.dialogueLines.Add(newLine);

            talkingManager.gameObject.SetActive(true);
            talkingManager.StartDialogueSequence(false);
        }
    }
}