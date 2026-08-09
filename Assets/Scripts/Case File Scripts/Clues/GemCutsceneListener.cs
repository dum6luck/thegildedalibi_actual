using UnityEngine;

public class GemCutsceneListener : MonoBehaviour
{
    [Header("UI Reference")]
    [Tooltip("Drag your dialogue or cutscene UI Canvas/Panel here (e.g., Clue_Canvas or cutscene_bg).")]
    [SerializeField] private GameObject cutsceneUIPanel;

    [Header("Settings")]
    [Tooltip("If TRUE: hides gem the moment the cutscene UI appears. If FALSE: hides gem when cutscene UI closes.")]
    [SerializeField] private bool hideWhenUIOpens = true;

    private bool wasUIActive = false;

    private void Awake()
    {
        // Force the gem to be visible when the scene starts playing
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (cutsceneUIPanel == null) return;

        bool isUIActive = cutsceneUIPanel.activeInHierarchy;

        if (hideWhenUIOpens)
        {
            // Only hide when the UI transitions from OFF -> ON
            if (isUIActive)
            {
                HideGem();
            }
        }
        else
        {
            // Track when UI opens, then hide when it turns back OFF
            if (isUIActive)
            {
                wasUIActive = true;
            }
            else if (wasUIActive && !isUIActive)
            {
                HideGem();
            }
        }
    }

    public void HideGem()
    {
        gameObject.SetActive(false);
    }
}