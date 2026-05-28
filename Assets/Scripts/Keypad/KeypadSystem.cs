using UnityEngine;
using TMPro;

public class KeypadSystem : MonoBehaviour
{
    [Header("Settings")]
    public string correctCode = "0";
    public GameObject keypadUI;

    [Header("References")]
    public TMP_InputField inputField;
    public TextMeshProUGUI statusText;

    [Header("Door Settings")]
    public GameObject doorObject;
    // Set this to (0, -6.454, 0) in the Inspector
    public Vector3 openRotation = new Vector3(0f, -6.454f, 0f);

    void Start()
    {
        ToggleKeypad(false);
    }

    void Update()
    {
        // Check if the keypad UI is currently open/visible
        if (keypadUI.activeSelf)
        {
            // FIX: If the player presses Escape OR E, close the keypad layout cleanly
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E))
            {
                ToggleKeypad(false);
            }
        }
    }

    public void CheckCode()
    {
        if (inputField.text == correctCode)
        {
            statusText.text = "<color=green>CORRECT</color>";

            // This snaps the door to the exact rotation from your screenshot
            if (doorObject != null)
            {
                doorObject.transform.localRotation = Quaternion.Euler(openRotation);
            }

            Invoke("UnlockSuccess", 1.0f);
        }
        else
        {
            statusText.text = "<color=red>ACCESS DENIED</color>";
            inputField.text = "";
            inputField.ActivateInputField();
        }
    }

    public void ToggleKeypad(bool state)
    {
        keypadUI.SetActive(state);
        Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = state;

        if (state)
        {
            statusText.text = "ENTER CODE";
            inputField.text = "";
            inputField.ActivateInputField();
        }
        else
        {
            // Clean up focus parameters when closing so the input field doesn't trap keystrokes
            if (inputField != null && inputField.isFocused)
            {
                inputField.DeactivateInputField();
            }
        }
    }

    void UnlockSuccess()
    {
        ToggleKeypad(false);
    }
}