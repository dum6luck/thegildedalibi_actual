using UnityEngine;

public class LensDataCarrier : MonoBehaviour
{
    public static LensDataCarrier Instance { get; private set; }

    [Header("Saved States")]
    public bool isBlueLensUnlocked = false;
    public bool isRedLensUnlocked = false;
    public bool isBlackLensUnlocked = true;
    public bool hasTriggeredFirstLensTutorial = false;

    [Header("Timer Tracking")]
    public float blueTimerRemaining = 0f;
    public float redTimerRemaining = 0f;

    // !!! NEW: Tracks which lens name string is actively turned on !!!
    public string activeLensLayerName = "None";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}