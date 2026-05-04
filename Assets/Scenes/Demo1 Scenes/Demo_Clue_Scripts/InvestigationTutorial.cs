using UnityEngine;
using TMPro;

public class InvestigationTutorial : MonoBehaviour
{
    [Header("UI References")]
    public GameObject magnifyingGlassUI;
    public GameObject blueOverlay;
    public GameObject redOverlay;        // New: Red Tint child
    public GameObject blacklightOverlay; // New: Blacklight/Purple Tint child
    public TextMeshProUGUI tutorialInstructionText;
    public Talking_Manager talkingManager;

    [Header("Lens System (Cameras/Lights)")]
    public GameObject blueLensCamera;
    public GameObject redLensCamera;        // New: For detecting red clues
    public GameObject blacklightLensCamera; // New: For detecting hidden prints

    private int tutorialStep = 0;

    void Start()
    {
        magnifyingGlassUI.SetActive(false);
        DeactivateAllLenses();

        tutorialInstructionText.text = "Press Q to take out magnifying glass.";
    }

    void Update()
    {
        // Handle Lens Switching (Available once the magnifying glass is out)
        if (magnifyingGlassUI.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) ActivateLens("blue");
            if (Input.GetKeyDown(KeyCode.Alpha2)) ActivateLens("red");
            if (Input.GetKeyDown(KeyCode.Alpha3)) ActivateLens("blacklight");
        }

        // Tutorial Progression Logic
        switch (tutorialStep)
        {
            case 0: // Waiting for Q
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    magnifyingGlassUI.SetActive(true);
                    tutorialInstructionText.text = "Press I to examine body.";
                    tutorialStep = 1;
                }
                break;

            case 1: // Waiting for I
                if (Input.GetKeyDown(KeyCode.I))
                {
                    if (talkingManager != null)
                    {
                        talkingManager.gameObject.SetActive(true);
                        talkingManager.StartDialogueSequence(false);
                        tutorialInstructionText.text = "";
                        tutorialStep = 2;
                    }
                }
                break;

            case 2: // Waiting for dialogue to end
                if (talkingManager != null && !talkingManager.gameObject.activeSelf)
                {
                    tutorialInstructionText.text = "Press 1, 2, or 3 to change lenses.";
                    tutorialStep = 3;
                }
                break;
        }
    }

    void ActivateLens(string lensType)
    {
        DeactivateAllLenses();

        switch (lensType)
        {
            case "blue":
                if (blueLensCamera != null) blueLensCamera.SetActive(true);
                if (blueOverlay != null) blueOverlay.SetActive(true);
                break;
            case "red":
                if (redLensCamera != null) redLensCamera.SetActive(true);
                if (redOverlay != null) redOverlay.SetActive(true);
                break;
            case "blacklight":
                if (blacklightLensCamera != null) blacklightLensCamera.SetActive(true);
                if (blacklightOverlay != null) blacklightOverlay.SetActive(true);
                break;
        }

        if (tutorialStep == 3)
        {
            tutorialInstructionText.text = "Lens switched!";
            Invoke("ClearInstructions", 2f);
        }
    }

    void DeactivateAllLenses()
    {
        if (blueLensCamera != null) blueLensCamera.SetActive(false);
        if (redLensCamera != null) redLensCamera.SetActive(false);
        if (blacklightLensCamera != null) blacklightLensCamera.SetActive(false);

        if (blueOverlay != null) blueOverlay.SetActive(false);
        if (redOverlay != null) redOverlay.SetActive(false);
        if (blacklightOverlay != null) blacklightOverlay.SetActive(false);
    }

    void ClearInstructions()
    {
        tutorialInstructionText.text = "";
    }
}