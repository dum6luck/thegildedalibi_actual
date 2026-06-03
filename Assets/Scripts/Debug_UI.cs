using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic; // Added for List support

/* 
 * SUMMARY:
 * This script manages the Case File interface. It handles pausing the game,
 * unlocking the cursor for UI interaction, and populating the clue list.
 * It now tracks a list of collected Clue_Data for NPCs to reference.
 */

public class Debug_UI : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject debug_panel;

    private bool is_open = false;

    private void Start()
    {
        debug_panel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Semicolon))
        {
            if (is_open)
            {
                Close_Debug();
            }
            else
            {
                Open_Debug();
            }
        }
    }

    public void Open_Debug()
    {
        debug_panel.SetActive(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        is_open = true;
    }

    public void Close_Debug()
    {
        debug_panel.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        is_open = false;
    }
}