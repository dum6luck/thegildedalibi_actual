using UnityEngine;

public class LensSystem : MonoBehaviour
{
    private Camera cam;
    private int baseMask;

    [Header("Magnifying Glass Frame")]
    public GameObject magGlassOverlay; // Drag your 'MagGlassOverlay' here

    [Header("Overlay UI Lenses")]
    public GameObject blueOverlay;
    public GameObject redOverlay;
    public GameObject blackOverlay;

    void Start()
    {
        cam = GetComponent<Camera>();
        baseMask = cam.cullingMask;
        ResetLens(); // Ensure everything starts clean
    }

    void Update()
    {
        // Toggle Logic for Key 1 (Blue)
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (blueOverlay.activeSelf) ResetLens();
            else SwitchLens("Blue Light", blueOverlay);
        }

        // Toggle Logic for Key 2 (Red)
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (redOverlay.activeSelf) ResetLens();
            else SwitchLens("Red Light", redOverlay);
        }

        // Toggle Logic for Key 3 (Black)
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (blackOverlay.activeSelf) ResetLens();
            else SwitchLens("Black Light", blackOverlay);
        }

        // Emergency manual reset key
        if (Input.GetKeyDown(KeyCode.Alpha0)) ResetLens();
    }

    void SwitchLens(string layerName, GameObject activeLens)
    {
        int layerIndex = LayerMask.NameToLayer(layerName);
        if (layerIndex == -1) return;

        // Clear previous states
        DisableAllOverlays();

        // 1. Change what the camera sees
        cam.cullingMask = baseMask | (1 << layerIndex);

        // 2. Turn on the main magnifying glass frame
        if (magGlassOverlay != null) magGlassOverlay.SetActive(true);

        // 3. Turn on the specific colored lens inside the glass
        if (activeLens != null) activeLens.SetActive(true);

        Debug.Log($"Lens Active: {layerName}");
    }

    public void ResetLens()
    {
        cam.cullingMask = baseMask;
        DisableAllOverlays();
        Debug.Log("Lenses and Magnifying Glass Hidden");
    }

    void DisableAllOverlays()
    {
        // Hide the main frame
        if (magGlassOverlay) magGlassOverlay.SetActive(false);

        // Hide individual color lenses
        if (blueOverlay) blueOverlay.SetActive(false);
        if (redOverlay) redOverlay.SetActive(false);
        if (blackOverlay) blackOverlay.SetActive(false);
    }
}