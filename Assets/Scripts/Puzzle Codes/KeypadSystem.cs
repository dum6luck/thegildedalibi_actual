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
        if (keypadUI.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleKeypad(false);
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
    }

    void UnlockSuccess()
    {
        ToggleKeypad(false);
    }
}