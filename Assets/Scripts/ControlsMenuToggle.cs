using UnityEngine;
using UnityEngine.UI;

public class ControlsMenuToggle : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject controlsMenuPanel;

    [Header("Movement Speed Adjuster")]
    public GameObject playerObject;
    public Slider speedSlider;

    [Header("Keybind Settings")]
    public KeyCode toggleKey = KeyCode.M;

    private FPSController movementScript;

    void Start()
    {
        if (controlsMenuPanel != null) controlsMenuPanel.SetActive(false);
        FindPlayerScript();
    }

    void Update()
    {
        // Listens for the M key press to open/close the menu
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleMenu();
        }
    }

    void FindPlayerScript()
    {
        // Try finding player by field assignment first, or search tag/type if null
        if (playerObject == null)
        {
            playerObject = GameObject.FindGameObjectWithTag("Player");
        }

        if (playerObject != null)
        {
            movementScript = playerObject.GetComponent<FPSController>();
            if (movementScript != null)
            {
                InitializeSlider();
            }
        }
    }

    void InitializeSlider()
    {
        if (speedSlider == null || movementScript == null) return;

        speedSlider.minValue = 5f;
        speedSlider.maxValue = 25f;
        speedSlider.value = Mathf.Clamp(movementScript.walkSpeed, speedSlider.minValue, speedSlider.maxValue);

        // Stops the slider from stealing WASD arrow keys focus
        speedSlider.navigation = new Navigation { mode = Navigation.Mode.None };

        speedSlider.onValueChanged.RemoveAllListeners();
        speedSlider.onValueChanged.AddListener(OnSpeedSliderChanged);
    }

    public void OnSpeedSliderChanged(float newValue)
    {
        if (movementScript == null) return;
        if (newValue < 4f) newValue = 5f;

        // Automatically updates walk and sprint speed values live
        movementScript.walkSpeed = newValue;
        movementScript.sprintSpeed = newValue * 1.8f;
    }

    public void ToggleMenu()
    {
        if (movementScript == null) FindPlayerScript();

        if (controlsMenuPanel != null)
        {
            bool isCurrentlyActive = controlsMenuPanel.activeSelf;
            bool nextState = !isCurrentlyActive;

            controlsMenuPanel.SetActive(nextState);

            if (nextState == false) // Menu closing: lock mouse back into game
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else // Menu opening: free mouse to use slider
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}