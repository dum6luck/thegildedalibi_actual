using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CaseFileUI : MonoBehaviour
{
    [Header("UI References")]
    public Image clueImage;
    public TMP_Text clueTitle;
    public TMP_Text clueDescription;

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