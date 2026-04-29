using UnityEngine;

public class MingleSceneManager : MonoBehaviour
{
    [Header("References")]
    public Talking_Manager talkingManager;

    [Tooltip("Drag your Player object (with the FPSController script) here")]
    public FPSController playerController;

    void Start()
    {
        // 1. Initial Setup: Freeze the player and camera
        if (playerController != null)
        {
            playerController.canMove = false;
            playerController.canLook = false;

            // Unlock the cursor so the player can click the dialogue/next arrow
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // 2. Start the intro monologue
        if (talkingManager != null)
        {
            talkingManager.StartDialogueSequence();
        }
        else
        {
            Debug.LogError("MingleSceneManager: Talking Manager is not assigned!");
        }
    }

    void Update()
    {
        // 3. Check if the dialogue is finished
        // We know it's finished because Talking_Manager disables its own object at the end
        if (talkingManager != null && !talkingManager.gameObject.activeSelf)
        {
            // Only run this if the player is still frozen
            if (playerController != null && !playerController.canMove)
            {
                UnlockExploration();
            }
        }
    }

    void UnlockExploration()
    {
        playerController.canMove = true;
        playerController.canLook = true;

        // Re-lock the cursor for first-person movement
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("Mingle Phase: Detective is finished thinking. Exploration enabled.");
    }
}