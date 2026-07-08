using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/* * SUMMARY:
 * Definitive Detective Vision Controller for Unity 2021 URP.
 * Directly exposes Feature Target Name text boxes to the Inspector
 * to cleanly handle disabling features on startup.
 */

public class DetectiveVisionController : MonoBehaviour
{
    [Header("URP Renderer Link")]
    [Tooltip("Drag your Universal Renderer Data asset here (Search 't:UniversalRendererData' in Project tab)")]
    public UniversalRendererData rendererData;

    [Header("Feature Target Names")]
    [Tooltip("Type the exact name of your behind-walls feature layer here")]
    public string behindWallFeatureName = "HighlightOpaque";
    [Tooltip("Type the exact name of your in-sight feature layer here")]
    public string inSightFeatureName = "NormalOpaque";

    [Header("Screen Dimming (Post Processing)")]
    [Tooltip("Drag your Detective_Dim_Volume game object here")]
    public Volume detectiveVisionVolume;

    [Tooltip("How fast the screen dims/brightens when entering vision mode")]
    public float transitionSpeed = 7f;

    private ScriptableRendererFeature behindWallFeature;
    private ScriptableRendererFeature inSightFeature;
    private float targetWeight = 0f;

    void Start()
    {
        FindRendererFeatures();

        // Force the features completely OFF the moment the game boots
        SetVisionFeaturesActive(false);
        if (detectiveVisionVolume != null) detectiveVisionVolume.weight = 0f;
    }

    void Update()
    {
        // 1. Hold 'V' to turn on detective mode
        if (Input.GetKeyDown(KeyCode.V))
        {
            targetWeight = 1f;
            SetVisionFeaturesActive(true);
        }

        // 2. Release 'V' to turn it back off
        if (Input.GetKeyUp(KeyCode.V))
        {
            targetWeight = 0f;
            SetVisionFeaturesActive(false);
        }

        // 3. Smoothly blend the post-processing dimming volume weight
        if (detectiveVisionVolume != null)
        {
            detectiveVisionVolume.weight = Mathf.Lerp(detectiveVisionVolume.weight, targetWeight, Time.deltaTime * transitionSpeed);
        }
    }

    void SetVisionFeaturesActive(bool active)
    {
        if (behindWallFeature != null) behindWallFeature.SetActive(active);
        if (inSightFeature != null) inSightFeature.SetActive(active);
    }

    void FindRendererFeatures()
    {
        if (rendererData == null)
        {
            Debug.LogError("[Detective Vision] Missing Universal Renderer Data asset in the script slot!");
            return;
        }

        // Search your URP data asset for your custom feature layer strings
        foreach (var feature in rendererData.rendererFeatures)
        {
            if (feature != null)
            {
                if (feature.name == behindWallFeatureName) behindWallFeature = feature;
                if (feature.name == inSightFeatureName) inSightFeature = feature;
            }
        }
    }

    private void OnDisable()
    {
        SetVisionFeaturesActive(false);
    }

    private void OnDestroy()
    {
        // Safety clean up to reset your editor asset state when exiting play mode
        SetVisionFeaturesActive(false);
    }
}