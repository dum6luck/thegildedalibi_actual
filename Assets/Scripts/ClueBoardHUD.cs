using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Persistent top-right HUD icon that opens the Clue Board scene.
/// Assign IconRoot and CountLabel from the scene Canvas in the Inspector.
/// The icon is hidden while the inspection screen or Julian's dialogue is open.
/// </summary>
public class ClueBoardHUD : MonoBehaviour
{
    public static ClueBoardHUD Instance { get; private set; }

    [Header("UI References — assign from scene Canvas")]
    [Tooltip("The icon button root to show/hide.")]
    public GameObject IconRoot;

    [Tooltip("The badge label showing the clue count.")]
    public TextMeshProUGUI CountLabel;

    private const string ClueBoardScene = "ClueBoard";
    private bool boardOpen = false;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        ClueInventory.OnClueAdded += OnClueAdded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        ClueInventory.OnClueAdded -= OnClueAdded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void Start()
    {
        RefreshCount();
    }

    private void Update()
    {
        // Hide icon while other full-screen UIs are open
        bool shouldShow = !JulianDialogue.IsOpen && !InspectionScreen.IsOpen && !boardOpen;
        if (IconRoot != null && IconRoot.activeSelf != shouldShow)
            IconRoot.SetActive(shouldShow);
    }

    /// <summary>Called by the icon button's OnClick.</summary>
    public void OnIconClicked()
    {
        if (boardOpen) return;
        boardOpen = true;
        SceneManager.LoadSceneAsync(ClueBoardScene, LoadSceneMode.Additive);
    }

    private void OnClueAdded(string _) => RefreshCount();

    private void OnSceneUnloaded(Scene scene)
    {
        if (scene.name == ClueBoardScene)
            boardOpen = false;
    }

    private void RefreshCount()
    {
        int count = ClueInventory.Instance != null ? ClueInventory.Instance.Clues.Count : 0;
        if (CountLabel != null)
            CountLabel.text = count.ToString();
    }
}
