using UnityEngine;
using TMPro;

/* * SUMMARY:
 * This script handles the lock code logic, door animation, and menu state.
 * All automatic proximity detection has been stripped; it relies entirely
 * on the PlayerKeypadInteractor script shooting a raycast at the door.
 */

public class KeypadSystem : MonoBehaviour
{
    [Header("Settings")]
    public string correctCode = "4729";
    public GameObject keypadUI;

    [Header("References")]
    public TMP_InputField inputField;
    public TextMeshProUGUI statusText;

    [Header("Player Inventory System")]
    [Tooltip("Drag your main Player object here so they automatically get the key.")]
    public GameObject playerObject;

    [Header("Door Settings")]
    public GameObject doorObject;
    public Vector3 openRotation = new Vector3(0f, -6.454f, 0f);

    [Header("Unlock Dialogue")]
    public string detectiveName = "DETECTIVE";
    [TextArea(3, 10)]
    public string unlockThoughts = "A key... maybe I can use it somewhere.";

    void Start()
    {
        // Enforce safe default closure state on initialization
        ToggleKeypad(false);
    }

    void Update()
    {
        // Only run code tracking if the UI layer is currently active
        if (keypadUI.activeSelf)
        {
            // Allow cancelling out manually
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E))
            {
                ToggleKeypad(false);
                return;
            }

            if (inputField != null && inputField.isFocused)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    CheckCode();
                }
            }
        }
    }

    public void CheckCode()
    {
        if (inputField.text == correctCode)
        {
            statusText.text = "<color=green>CORRECT</color>";

            // 1. Grant the key token to player
            if (playerObject != null)
            {
                PlayerInventory inventory = playerObject.GetComponent<PlayerInventory>();
                if (inventory != null)
                {
                    inventory.PickUpKey();
                }
            }

            // 2. Open the physical container
            if (doorObject != null)
            {
                doorObject.transform.localRotation = Quaternion.Euler(openRotation);
            }

            // 3. Trigger dialogue text banner
            if (Dialogue_Manager.Instance != null)
            {
                Dialogue_Manager.Instance.Show_Dialogue(detectiveName, unlockThoughts);
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

        // Toggles game timescale to freeze background action while interfacing
        Time.timeScale = state ? 0f : 1f;

        if (state)
        {
            statusText.text = "ENTER CODE";
            inputField.text = "";
            inputField.ActivateInputField();
        }
        else
        {
            if (inputField != null)
            {
                inputField.DeactivateInputField();
                UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(null);
            }
        }
    }

    void UnlockSuccess()
    {
        ToggleKeypad(false);
    }
}