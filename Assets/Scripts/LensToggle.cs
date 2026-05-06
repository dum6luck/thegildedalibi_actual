using UnityEngine;

public class LensToggle : MonoBehaviour
{
    [Header("References")]
    public GameObject mainCamera; // Drag your BlueLensCamera here
    public GameObject blueLensCamera; // Drag your BlueLensCamera here

    void Start()
    {
        // Ensure the lens is off when the game starts
        if (blueLensCamera != null)
            blueLensCamera.SetActive(false);
    }

    void Update()
    {
        // Toggle with the '1' key
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            bool currentState = blueLensCamera.activeSelf;
            blueLensCamera.SetActive(!currentState);
            mainCamera.SetActive(currentState);
            
            Debug.Log("Blue Light Lens: " + (!currentState ? "ON" : "OFF"));
        }
    }
}