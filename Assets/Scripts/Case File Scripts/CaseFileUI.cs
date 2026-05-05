using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CaseFileUI : MonoBehaviour
{
    [Header("UI References")]
    public Image clueImage;
    public TMP_Text clueTitle;
    public TMP_Text clueDescription;

    [Header("Grid Setup")]
    public GameObject clueButtonPrefab; // A button with an image
    public Transform gridParent;       // The Content of your Scroll View

    void OnEnable() // Runs every time you open the Case File
    {
        PopulateClueList();
    }

    public void PopulateClueList()
    {
        // Clear old buttons first
        foreach (Transform child in gridParent) Destroy(child.gameObject);

        // Create a button for every clue in the ClueManager
        foreach (Clue c in ClueManager.Instance.collectedClues)
        {
            GameObject btn = Instantiate(clueButtonPrefab, gridParent);

            // Set the icon on the button
            Image icon = btn.GetComponentInChildren<Image>();
            if (icon != null) icon.sprite = c.clueImage;

            // Make the button show the details in your existing DisplayClue function
            btn.GetComponent<Button>().onClick.AddListener(() => DisplayClue(c));
        }
    }

    void Start()
    {
        ClearUI();
    }

    public void DisplayClue(Clue clue)
    {
        if (clue == null) return;

        // Text
        clueTitle.text = clue.clueName;
        clueDescription.text = clue.clueDescription;

        // IMAGE FIX (important part)
        if (clue.clueImage != null)
        {
            clueImage.gameObject.SetActive(true);
            clueImage.enabled = true;
            clueImage.color = Color.white; // ensures it’s visible
            clueImage.sprite = clue.clueImage;
            clueImage.preserveAspect = true;
        }
        else
        {
            clueImage.sprite = null;
            clueImage.gameObject.SetActive(false);
        }

        // Force UI refresh (VERY important in Unity UI systems)
        LayoutRebuilder.ForceRebuildLayoutImmediate(
            clueImage.rectTransform.parent as RectTransform
        );
    }

    public void ClearUI()
    {
        clueTitle.text = "";
        clueDescription.text = "Select a clue to inspect.";

        clueImage.sprite = null;
        clueImage.color = Color.white;
        clueImage.enabled = false;
        clueImage.gameObject.SetActive(false);
    }
}