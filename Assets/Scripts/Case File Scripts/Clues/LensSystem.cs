using UnityEngine;
using UnityEngine.UI; // Needed for the Image component

public class LensSystem : MonoBehaviour
{
    private Camera cam;
    private int baseMask;

    [Header("Overlay UI Elements")]
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
        // Toggle logic: If the lens is already active, Reset. Otherwise, Switch.
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (blueOverlay.activeSelf) ResetLens();
            else SwitchLens("Blue Light", blueOverlay);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (redOverlay.activeSelf) ResetLens();
            else SwitchLens("Red Light", redOverlay);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (blackOverlay.activeSelf) ResetLens();
            else SwitchLens("Black Light", blackOverlay);
        }

        if (Input.GetKeyDown(KeyCode.Alpha0)) ResetLens();
    }

    void SwitchLens(string layerName, GameObject activeOverlay)
    {
        int layerIndex = LayerMask.NameToLayer(layerName);

        if (layerIndex == -1) return;

        // Clean slate before applying the new lens
        DisableAllOverlays();

        // Activate the specific lens layer and UI
        cam.cullingMask = baseMask | (1 << layerIndex);
        if (activeOverlay != null) activeOverlay.SetActive(true);
    }

    public void ResetLens()
    {
        cam.cullingMask = baseMask;
        DisableAllOverlays();
        Debug.Log("Lenses Reset to Default");
    }

    void DisableAllOverlays()
    {
        if (blueOverlay) blueOverlay.SetActive(false);
        if (redOverlay) redOverlay.SetActive(false);
        if (blackOverlay) blackOverlay.SetActive(false);
    }
}