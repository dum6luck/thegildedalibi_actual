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
    public GameObject screenCluePrompt;

    [Header("Lens Timers")]
    public float maxTime = 20f;
    private float blueTimer = 0f;
    private float redTimer = 0f;

    [Header("Unlock Progression")]
    public bool isBlueLensUnlocked = false;
    public bool isRedLensUnlocked = false;
    public bool isBlackLensUnlocked = true;

    [Header("First Time Event")]
    private bool hasTriggeredFirstLensTutorial = false;

    private string currentActiveLensName = "None";

    void Start()
    {
        cam = GetComponent<Camera>();
        baseMask = cam.cullingMask;

        if (lensTimerSlider != null)
        {
            lensTimerSlider.gameObject.SetActive(false);
        }

        if (LensDataCarrier.Instance != null)
        {
            isBlueLensUnlocked = LensDataCarrier.Instance.isBlueLensUnlocked;
            isRedLensUnlocked = LensDataCarrier.Instance.isRedLensUnlocked;
            isBlackLensUnlocked = LensDataCarrier.Instance.isBlackLensUnlocked;
            hasTriggeredFirstLensTutorial = LensDataCarrier.Instance.hasTriggeredFirstLensTutorial;

            blueTimer = LensDataCarrier.Instance.blueTimerRemaining;
            redTimer = LensDataCarrier.Instance.redTimerRemaining;
            currentActiveLensName = LensDataCarrier.Instance.activeLensLayerName;

            if (currentActiveLensName == "Blue Light" && blueTimer > 0)
                SwitchLens("Blue Light", blueOverlay, true);
            else if (currentActiveLensName == "Red Light" && redTimer > 0)
                SwitchLens("Red Light", redOverlay, true);
            else if (currentActiveLensName == "Black Light")
                SwitchLens("Black Light", blackOverlay, false);
            else
                ResetLens();
        }
        else
        {
            ResetLens();
        }
    }

    void Update()
    {
        // --- FIXED MANUAL TOGGLES ---
        // Added '&& blueTimer > 0' and '&& redTimer > 0' so they CANNOT be opened with an empty battery!
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (isBlueLensUnlocked && blueTimer > 0)
            {
                HandleManualToggle("Blue Light", blueOverlay, ref blueTimer);
            }
            else if (!isBlueLensUnlocked)
            {
                Debug.Log("Blue Light Lens is locked! Find the Blue Gem first.");
            }
            else
            {
                Debug.Log("Blue Light battery is dead! Go back to the Blue Gem to recharge.");
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (isRedLensUnlocked && redTimer > 0)
            {
                HandleManualToggle("Red Light", redOverlay, ref redTimer);
            }
            else if (!isRedLensUnlocked)
            {
                Debug.Log("Red Light Lens is locked! Find the Red Gem first.");
            }
            else
            {
                Debug.Log("Red Light battery is dead! Go back to the Red Gem to recharge.");
            }
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

        // --- BACKGROUND COUNTDOWNS ---
        bool aTimerIsCountingDown = false;

        if (blueTimer > 0)
        {
            blueTimer -= Time.deltaTime;
            aTimerIsCountingDown = true;
            UpdateSliderUI(blueTimer);

            if (blueTimer <= 0)
            {
                blueTimer = 0f;
                ResetLens();
            }
        }

        if (redTimer > 0)
        {
            redTimer -= Time.deltaTime;
            aTimerIsCountingDown = true;
            UpdateSliderUI(redTimer);

            if (redTimer <= 0)
            {
                redTimer = 0f;
                ResetLens();
            }
        }

        if (!aTimerIsCountingDown && lensTimerSlider != null && lensTimerSlider.gameObject.activeSelf)
        {
            lensTimerSlider.gameObject.SetActive(false);
        }

        SaveDataToCarrier();
    }

    void SaveDataToCarrier()
    {
        if (LensDataCarrier.Instance != null)
        {
            LensDataCarrier.Instance.isBlueLensUnlocked = isBlueLensUnlocked;
            LensDataCarrier.Instance.isRedLensUnlocked = isRedLensUnlocked;
            LensDataCarrier.Instance.isBlackLensUnlocked = isBlackLensUnlocked;
            LensDataCarrier.Instance.hasTriggeredFirstLensTutorial = hasTriggeredFirstLensTutorial;

            LensDataCarrier.Instance.blueTimerRemaining = blueTimer;
            LensDataCarrier.Instance.redTimerRemaining = redTimer;
            LensDataCarrier.Instance.activeLensLayerName = currentActiveLensName;
        }
    }

    public void ShowCluePrompt()
    {
        if (screenCluePrompt != null) screenCluePrompt.SetActive(true);
    }

    public void HideCluePrompt()
    {
        if (screenCluePrompt != null) screenCluePrompt.SetActive(false);
    }

    void UpdateSliderUI(float currentTimerValue)
    {
        if (lensTimerSlider != null)
        {
            if (!lensTimerSlider.gameObject.activeSelf)
            {
                lensTimerSlider.gameObject.SetActive(true);
            }

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

        if (lensTimerSlider != null && showTimer)
        {
            lensTimerSlider.gameObject.SetActive(true);
        }

        currentActiveLensName = layerName;

        if (!hasTriggeredFirstLensTutorial && (layerName == "Blue Light" || layerName == "Red Light"))
        {
            TriggerJulianTutorialDialogue();
        }
    }

    void TriggerJulianTutorialDialogue()
    {
        Dialogue_Manager dialogue_manager = FindObjectOfType<Dialogue_Manager>();

        if (dialogue_manager != null)
        {
            hasTriggeredFirstLensTutorial = true;
            if (LensDataCarrier.Instance != null) LensDataCarrier.Instance.hasTriggeredFirstLensTutorial = true;

            dialogue_manager.Show_Dialogue("JULIAN", "Hey, detective! Over here! Did you find something with that magnifying glass? Come check out this mask, something looks strange about it.");
        }
    }

    public void ResetLens()
    {
        cam.cullingMask = baseMask;
        currentActiveLensName = "None";
        DisableAllOverlays();
    }

    void DisableAllOverlays()
    {
        if (magGlassOverlay) magGlassOverlay.SetActive(false);
        if (blueOverlay) blueOverlay.SetActive(false);
        if (redOverlay) redOverlay.SetActive(false);
        if (blackOverlay) blackOverlay.SetActive(false);

        HideCluePrompt();
    }
}