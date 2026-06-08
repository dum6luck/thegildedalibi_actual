using UnityEngine;
using UnityEngine.UI;

public class LensSystem : MonoBehaviour
{
    private Camera cam;
    private int baseMask;

    [Header("Magnifying Glass Frame")]
    public GameObject magGlassOverlay;

    [Header("Overlay UI Lenses")]
    public GameObject blueOverlay;
    public GameObject redOverlay;
    public GameObject blackOverlay;

    [Header("Timer UI Element")]
    public Slider lensTimerSlider;

    [Header("Screen-Space Clue Prompt")]
    public GameObject screenCluePrompt; // Drag 'ScreenInteractionPrompt' here

    [Header("Lens Timers")]
    public float maxTime = 30f;
    private float blueTimer = 0f;
    private float redTimer = 0f;

    [Header("Unlock Progression")]
    public bool isBlueLensUnlocked = false;
    public bool isRedLensUnlocked = false;
    public bool isBlackLensUnlocked = true;

    void Start()
    {
        cam = GetComponent<Camera>();
        baseMask = cam.cullingMask;
        ResetLens();
    }

    void Update()
    {
        // 1. Manual Key Toggles
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (isBlueLensUnlocked) HandleManualToggle("Blue Light", blueOverlay, ref blueTimer);
            else Debug.Log("Blue Light Lens is locked! Find the Blue Gem first.");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (isRedLensUnlocked) HandleManualToggle("Red Light", redOverlay, ref redTimer);
            else Debug.Log("Red Light Lens is locked! Find the Red Gem first.");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (isBlackLensUnlocked)
            {
                float dummyTimer = 0f;
                HandleManualToggle("Black Light", blackOverlay, ref dummyTimer);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha0)) ResetLens();

        // 2. Countdown Timers
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

    // --- NEW PUBLIC FUNCTIONS CALLED BY CLUES ---
    public void ShowCluePrompt()
    {
        if (screenCluePrompt != null)
        {
            screenCluePrompt.SetActive(true);
        }
    }

    public void HideCluePrompt()
    {
        if (screenCluePrompt != null)
        {
            screenCluePrompt.SetActive(false);
        }
    }

    void UpdateSliderUI(float currentTimerValue)
    {
        if (lensTimerSlider != null)
        {
            lensTimerSlider.value = currentTimerValue / maxTime;
        }
    }

    public void ChargeLens(string lensType)
    {
        if (lensType == "Blue")
        {
            isBlueLensUnlocked = true;
            blueTimer = maxTime;
            SwitchLens("Blue Light", blueOverlay, true);
        }
        else if (lensType == "Red")
        {
            isRedLensUnlocked = true;
            redTimer = maxTime;
            SwitchLens("Red Light", redOverlay, true);
        }
    }

    void HandleManualToggle(string layerName, GameObject overlay, ref float timer)
    {
        if (overlay.activeSelf) ResetLens();
        else SwitchLens(layerName, overlay, (timer > 0));
    }

    void SwitchLens(string layerName, GameObject activeLens, bool showTimer = false)
    {
        int layerIndex = LayerMask.NameToLayer(layerName);
        if (layerIndex == -1) return;

        DisableAllOverlays();
        cam.cullingMask = baseMask | (1 << layerIndex);

        if (magGlassOverlay != null) magGlassOverlay.SetActive(true);
        if (activeLens != null) activeLens.SetActive(true);
        if (lensTimerSlider != null) lensTimerSlider.gameObject.SetActive(showTimer);
    }

    public void ResetLens()
    {
        cam.cullingMask = baseMask;
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

        // Hide prompt whenever resetting or switching lenses out
        HideCluePrompt();
    }
}