using UnityEngine;
using UnityEngine.UI; // Required for using Slider components

public class LensSystem : MonoBehaviour
{
    private Camera cam;
    private int baseMask;

    [Header("Magnifying Glass Frame")]
    public GameObject magGlassOverlay; // Drag 'MagGlassOverlay' here

    [Header("Overlay UI Lenses")]
    public GameObject blueOverlay;
    public GameObject redOverlay;
    public GameObject blackOverlay;

    [Header("Timer UI Element")]
    public Slider lensTimerSlider; // Drag your UI Slider here

    [Header("Lens Timers")]
    public float maxTime = 30f;
    private float blueTimer = 0f;
    private float redTimer = 0f;

    [Header("Unlock Progression")]
    public bool isBlueLensUnlocked = false;
    public bool isRedLensUnlocked = false;
    public bool isBlackLensUnlocked = true; // Set to true if Black Light is unlocked from the start

    void Start()
    {
        cam = GetComponent<Camera>();

        // Grab the camera's starting culling mask layers (Default, UI, etc.)
        baseMask = cam.cullingMask;

        ResetLens(); // Ensure a completely clean slate on game startup
    }

    void Update()
    {
        // 1. Manual Key Toggles
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (isBlueLensUnlocked) HandleManualToggle("Blue Light", blueOverlay, ref blueTimer);
            else Debug.Log("Blue Light Lens is locked! Go find the Blue Gem first.");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (isRedLensUnlocked) HandleManualToggle("Red Light", redOverlay, ref redTimer);
            else Debug.Log("Red Light Lens is locked! Go find the Red Gem first.");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (isBlackLensUnlocked)
            {
                float dummyTimer = 0f; // Black light uses no timer, pass a throwaway variable
                HandleManualToggle("Black Light", blackOverlay, ref dummyTimer);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha0)) ResetLens();

        // 2. Countdown Timers & Progress Bar Processing
        if (blueTimer > 0 && blueOverlay.activeSelf)
        {
            blueTimer -= Time.deltaTime;
            UpdateSliderUI(blueTimer);

            if (blueTimer <= 0) ResetLens();
        }

        if (redTimer > 0 && redOverlay.activeSelf)
        {
            redTimer -= Time.deltaTime;
            UpdateSliderUI(redTimer);

            if (redTimer <= 0) ResetLens();
        }
    }

    // Handles calculating and assigning the countdown slider percentage
    void UpdateSliderUI(float currentTimerValue)
    {
        if (lensTimerSlider != null)
        {
            lensTimerSlider.value = currentTimerValue / maxTime;
        }
    }

    // Triggered externally by LensGem.cs when the player stands nearby and presses 'E'
    public void ChargeLens(string lensType)
    {
        if (lensType == "Blue")
        {
            isBlueLensUnlocked = true;
            blueTimer = maxTime;
            SwitchLens("Blue Light", blueOverlay, true); // True tells it to reveal the timer bar
        }
        else if (lensType == "Red")
        {
            isRedLensUnlocked = true;
            redTimer = maxTime;
            SwitchLens("Red Light", redOverlay, true); // True tells it to reveal the timer bar
        }
    }

    void HandleManualToggle(string layerName, GameObject overlay, ref float timer)
    {
        if (overlay.activeSelf)
        {
            ResetLens();
        }
        else
        {
            // Show the timer bar only if this specific lens layer has remaining time left
            bool showTimer = (timer > 0);
            SwitchLens(layerName, overlay, showTimer);
        }
    }

    void SwitchLens(string layerName, GameObject activeLens, bool showTimer = false)
    {
        int layerIndex = LayerMask.NameToLayer(layerName);
        if (layerIndex == -1)
        {
            Debug.LogError($"Layer assignment failed! Unity cannot find a Layer named '{layerName}'.");
            return;
        }

        DisableAllOverlays();

        // Combine the baseline render mask with our temporary active detective frequency mask
        cam.cullingMask = baseMask | (1 << layerIndex);

        if (magGlassOverlay != null) magGlassOverlay.SetActive(true);
        if (activeLens != null) activeLens.SetActive(true);

        if (lensTimerSlider != null)
        {
            lensTimerSlider.gameObject.SetActive(showTimer);
        }
    }

    public void ResetLens()
    {
        cam.cullingMask = baseMask; // Restore standard vision layers
        blueTimer = 0f;
        redTimer = 0f;
        DisableAllOverlays();
    }

    void DisableAllOverlays()
    {
        if (magGlassOverlay) magGlassOverlay.SetActive(false);
        if (blueOverlay) blueOverlay.SetActive(false);
        if (redOverlay) redOverlay.SetActive(false);
        if (blackOverlay) blackOverlay.SetActive(false);
        if (lensTimerSlider) lensTimerSlider.gameObject.SetActive(false);
    }
}