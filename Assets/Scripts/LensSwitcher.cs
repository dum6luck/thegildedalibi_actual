using UnityEngine;
using UnityEngine.UI;

public class LensSwitcher : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Drag the MagnifyingGlassContainer (the parent) here.")]
    public GameObject magnifyingGlassContainer;

    [Header("Camera References")]
    public GameObject blueLensCamera;

    private bool isLensActive = false;

    void Start()
    {
        // 1. Ensure everything is OFF when the scene starts
        if (magnifyingGlassContainer != null)
            magnifyingGlassContainer.SetActive(false);

        if (blueLensCamera != null)
            blueLensCamera.SetActive(false);

        isLensActive = false;
    }

    void Update()
    {
        // Change 'Q' to whatever key you want to pull out the glass
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleLens();
        }
    }

    public void ToggleLens()
    {
        isLensActive = !isLensActive;

        // 2. Toggle the entire UI group (Mask, Tint, and Hand)
        if (magnifyingGlassContainer != null)
            magnifyingGlassContainer.SetActive(isLensActive);

        // 3. Toggle the camera at the same time
        if (blueLensCamera != null)
            blueLensCamera.SetActive(isLensActive);
    }
}