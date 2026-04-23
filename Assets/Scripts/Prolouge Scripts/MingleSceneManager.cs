using UnityEngine;

public class MingleSceneManager : MonoBehaviour
{
    [Header("References")]
    public Talking_Manager talkingManager;
    public FPSController playerController;

    [Header("Initial Setup")]
    public bool startWithDialogue = true;

    void Start()
    {
        if (startWithDialogue && talkingManager != null)
        {
            LockPlayer(true);
            talkingManager.gameObject.SetActive(true);
            // ADDED 'false' HERE
            talkingManager.StartDialogueSequence(false);
        }
        else
        {
            LockPlayer(false);
        }
    }

    void Update()
    {
        // Check every frame if the dialogue UI is active to freeze/unfreeze player
        if (talkingManager != null)
        {
            bool isDialogueActive = talkingManager.gameObject.activeSelf;

            // If UI is open but player is NOT locked, lock them
            if (isDialogueActive && (playerController.canMove || playerController.canLook))
            {
                LockPlayer(true);
            }
            // If UI is closed but player IS still locked, unlock them
            else if (!isDialogueActive && (!playerController.canMove || !playerController.canLook))
            {
                LockPlayer(false);
            }
        }
    }

    public void LockPlayer(bool isLocked)
    {
        if (playerController == null) return;

        if (isLocked)
        {
            // Disable movement and camera rotation
            playerController.canMove = false;
            playerController.canLook = false;

            // Show cursor for clicking the dialogue
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // Enable movement and camera rotation
            playerController.canMove = true;
            playerController.canLook = true;

            // Lock cursor back to center for FPS mode
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}