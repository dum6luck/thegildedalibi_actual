using UnityEngine;
using TMPro;

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
    // Set this to (0, -6.454, 0) in the Inspector
    public Vector3 openRotation = new Vector3(0f, -6.454f, 0f);

    [Header("Unlock Dialogue (New!)")]
    public string detectiveName = "DETECTIVE";
    [TextArea(3, 10)]
    public string unlockThoughts = "A key... maybe I can use it somewhere.";

    void Start()
    {
        ToggleKeypad(false);
    }

    void Update()
    {
        if (keypadUI.activeSelf)
        {
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

            // 1. Automatically grant the key to the player's inventory script
            if (playerObject != null)
            {
                PlayerInventory inventory = playerObject.GetComponent<PlayerInventory>();
                if (inventory != null)
                {
                    inventory.PickUpKey();
                }
            }

            // 2. Open the physical door
            if (doorObject != null)
            {
                doorObject.transform.localRotation = Quaternion.Euler(openRotation);
            }

            // 3. NEW: Trigger the dialogue box using your existing Dialogue_Manager setup!
            if (Dialogue_Manager.Instance != null)
            {
                Dialogue_Manager.Instance.Show_Dialogue(detectiveName, unlockThoughts);
            }
            else
            {
                Debug.LogWarning("Dialogue_Manager Instance not found in scene!");
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