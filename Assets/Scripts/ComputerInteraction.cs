using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class ComputerInteraction : MonoBehaviour
{
    [Header("Item Requirement")]
    [SerializeField] private string requiredItemID = "USB";
    [SerializeField] private bool consumeUSBOnUse = false;

    [Header("Scene Transition Settings")]
    [SerializeField] private string emailSceneName = "EmailScene";
    [SerializeField] private string accusationSceneName = "AccusationScene";
    [SerializeField] private bool savePlayerPositionBeforeTransition = true;

    [Header("Post-Email Dialogue")]
    [TextArea(2, 4)]
    [SerializeField] private string postEmailDialogue = "I've seen all I could. I think it's time to tie this up.";
    [SerializeField] private float delayBeforePostDialogue = 0.5f;

    [Header("Interaction Dialogue Content")]
    [TextArea(2, 4)]
    [SerializeField] private string missingUSBMessage = "The computer is locked. It looks like it requires a USB drive to access the files.";

    [TextArea(2, 4)]
    [SerializeField] private string insertingUSBMessage = "Inserting the USB drive into the terminal...";

    [Header("Interaction Prompt")]
    [SerializeField] private LensSystem cameraLensSystem;

    // Static flag persists across scene loads
    public static bool PendingPostEmailSequence = false;

    private Dialogue_Manager dialogueManager;
    private bool isInteracting = false;
    private bool isPlayerNearby = false;

    private IEnumerator Start()
    {
        dialogueManager = FindObjectOfType<Dialogue_Manager>();

        if (cameraLensSystem == null && Camera.main != null)
        {
            cameraLensSystem = Camera.main.GetComponent<LensSystem>();
        }

        // Check if returning from the EmailScene
        if (PendingPostEmailSequence)
        {
            PendingPostEmailSequence = false;
            yield return StartCoroutine(HandlePostEmailSequence());
        }
    }

    private void Update()
    {
        if (isPlayerNearby && !isInteracting && Input.GetKeyDown(KeyCode.E))
        {
            InteractWithComputer();
        }
    }

    public void InteractWithComputer()
    {
        if (isInteracting) return;

        bool hasUSB = (UserInventory.Instance != null && UserInventory.Instance.HasItem(requiredItemID));

        if (hasUSB)
        {
            StartCoroutine(HandleComputerAccessSequence());
        }
        else
        {
            StartCoroutine(HandleMissingUSBSequence());
        }
    }

    private IEnumerator HandleMissingUSBSequence()
    {
        isInteracting = true;

        if (dialogueManager != null)
        {
            dialogueManager.Show_Dialogue("DETECTIVE", missingUSBMessage);
        }

        yield return null;
        yield return null;

        while (dialogueManager != null && dialogueManager.Is_Dialogue_Ongoing())
        {
            if (Input.GetMouseButtonDown(0))
            {
                dialogueManager.SendMessage("SkipTypewriter", SendMessageOptions.DontRequireReceiver);
                dialogueManager.SendMessage("Finish_Typing", SendMessageOptions.DontRequireReceiver);
            }
            yield return null;
        }

        isInteracting = false;
    }

    private IEnumerator HandleComputerAccessSequence()
    {
        isInteracting = true;

        if (dialogueManager != null)
        {
            dialogueManager.Show_Dialogue("DETECTIVE", insertingUSBMessage);
        }

        yield return null;
        yield return null;

        bool dialogueFinished = false;
        while (!dialogueFinished)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (dialogueManager != null && dialogueManager.Is_Dialogue_Ongoing())
                {
                    dialogueManager.SendMessage("SkipTypewriter", SendMessageOptions.DontRequireReceiver);
                    dialogueManager.SendMessage("Finish_Typing", SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    dialogueFinished = true;
                }
            }
            yield return null;
        }

        if (consumeUSBOnUse && UserInventory.Instance != null)
        {
            UserInventory.Instance.RemoveItem(requiredItemID);
        }

        if (savePlayerPositionBeforeTransition)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                PlayerPositionManager.SavePosition(player.transform);
            }
        }

        // Set static flag so Start() catches it upon scene reload
        PendingPostEmailSequence = true;

        SceneManager.LoadScene(emailSceneName);
    }

    private IEnumerator HandlePostEmailSequence()
    {
        isInteracting = true;

        // Restore player position if saved
        if (PlayerPositionManager.HasSavedPosition)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                CharacterController controller = player.GetComponent<CharacterController>();
                if (controller != null) controller.enabled = false;

                player.transform.position = PlayerPositionManager.SavedPlayerPosition;
                player.transform.rotation = PlayerPositionManager.SavedPlayerRotation;

                if (controller != null) controller.enabled = true;

                PlayerPositionManager.ClearPosition();
            }
        }

        yield return new WaitForSeconds(delayBeforePostDialogue);

        if (dialogueManager != null)
        {
            dialogueManager.Show_Dialogue("DETECTIVE", postEmailDialogue);
        }

        yield return null;
        yield return null;

        // Wait for player click to advance dialogue
        bool dialogueFinished = false;
        while (!dialogueFinished)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
            {
                if (dialogueManager != null && dialogueManager.Is_Dialogue_Ongoing())
                {
                    dialogueManager.SendMessage("SkipTypewriter", SendMessageOptions.DontRequireReceiver);
                    dialogueManager.SendMessage("Finish_Typing", SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    dialogueFinished = true;
                }
            }
            yield return null;
        }

        // Load AccusationScene after the player clicks to close the dialogue
        SceneManager.LoadScene(accusationSceneName);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (cameraLensSystem != null) cameraLensSystem.ShowCluePrompt();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (cameraLensSystem != null) cameraLensSystem.HideCluePrompt();
        }
    }
}