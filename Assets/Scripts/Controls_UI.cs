using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic; // Added for List support

/* 
 * SUMMARY:
 * This script manages the Controls interface. It handles pausing the game,
 * unlocking the cursor for UI interaction, and giving instructions for playing.
 */

public class Controls_UI : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject controls_panel;
    public GameObject controls_background;

    private bool is_open = false;

    // NEW: This list stores the actual data objects you've found
    private List<Clue_Data> collected_clues_list = new List<Clue_Data>();

    private void Start()
    {
        controls_panel.SetActive(false);

        if (controls_background != null)
        {
            controls_background.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            if (is_open)
            {
                Close_Controls();
            }
            else
            {
                Open_Controls();
            }
        }
    }

    public void Open_Controls()
    {
        is_open = true;
        controls_panel.SetActive(true);

        if (controls_background != null)
        {
            controls_background.SetActive(true);
        }

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Close_Controls()
    {
        is_open = false;
        controls_panel.SetActive(false);

        if (controls_background != null)
        {
            controls_background.SetActive(false);
        }

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}