using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pauseMenuPanel;    // Drag your main pause menu here
    public GameObject controlsMenuPanel; // Drag your controls instructions panel here

    private bool isPaused = false;

    void Start()
    {
        // Make sure everything is hidden when the game starts
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (controlsMenuPanel != null) controlsMenuPanel.SetActive(false);
    }

    void Update()
    {
        // Detect Escape Key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f; // Freezes game time/physics

        // Unlock the mouse cursor so the player can click buttons
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isPaused = false;
        pauseMenuPanel.SetActive(false);
        controlsMenuPanel.SetActive(false); // Close controls if open
        Time.timeScale = 1f; // Unfreezes game time

        // Lock the mouse back to the game (assuming first-person/third-person setup)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // --- Button Functions ---

    public void OpenControls()
    {
        pauseMenuPanel.SetActive(false);   // Hide the main pause menu
        controlsMenuPanel.SetActive(true);  // Show the controls layout
    }

    public void CloseControls()
    {
        controlsMenuPanel.SetActive(false); // Hide the controls layout
        pauseMenuPanel.SetActive(true);    // Bring back main pause options
    }
}