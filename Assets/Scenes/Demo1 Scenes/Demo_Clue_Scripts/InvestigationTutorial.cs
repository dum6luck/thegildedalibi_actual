using UnityEngine;
using TMPro;

public class InvestigationTutorial : MonoBehaviour
{
    [Header("UI References")]
    public GameObject magnifyingGlassUI;
    public GameObject blueOverlay;
    public GameObject redOverlay;
    public GameObject blacklightOverlay;
    public GameObject caseFileUI; // Reference to your Evidence/Case Menu
    public TextMeshProUGUI tutorialInstructionText;
    public Talking_Manager talkingManager;

    [Header("Lens System (Cameras/Lights)")]
    public GameObject blueLensCamera;
    public GameObject redLensCamera;
    public GameObject blacklightLensCamera;

    private int tutorialStep = 0;
    private bool lensesUnlocked = false; // Prevents switching too early

    void Start()
    {
        magnifyingGlassUI.SetActive(false);
        if (caseFileUI != null) caseFileUI.SetActive(false);
        DeactivateAllLenses();

        tutorialInstructionText.text = "Press Q to take out magnifying glass.";
    }

    void Update()
    {
        // Lens Switching only works after the Case File step is completed
        if (magnifyingGlassUI.activeSelf && lensesUnlocked)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) ActivateLens("blue");
            if (Input.GetKeyDown(KeyCode.Alpha2)) ActivateLens("red");
            if (Input.GetKeyDown(KeyCode.Alpha3)) ActivateLens("blacklight");
        }

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
                        // Ensure the first line of your Talking_Manager list is the detective's quote:
                        // "Hmm, multiple stab wounds from the front..."
                        talkingManager.gameObject.SetActive(true);
                        talkingManager.StartDialogueSequence(false);
                        tutorialInstructionText.text = "";
                        tutorialStep = 2;
                    }
                }
                break;

            case 2: // Waiting for Body Dialogue to end
                if (talkingManager != null && !talkingManager.gameObject.activeSelf)
                {
                    tutorialInstructionText.text = "Press C to open Case File.";
                    tutorialStep = 3;
                }
                break;

            case 3: // Waiting for Case File (C)
                if (Input.GetKeyDown(KeyCode.C))
                {
                    if (caseFileUI != null)
                    {
                        caseFileUI.SetActive(true);
                        tutorialInstructionText.text = "Cause of death recorded.";
                        tutorialStep = 4;
                    }
                }
                break;

            case 4: // Final Step: Unlock Lenses
                // Wait for player to close the case file or just a small delay
                if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.Escape))
                {
                    if (caseFileUI != null) caseFileUI.SetActive(false);

                    lensesUnlocked = true;
                    tutorialInstructionText.text = "Lenses unlocked. Press 1, 2, or 3 to switch.";
                    tutorialStep = 5; // Tutorial complete
                }
                break;
        }
    }

    // (ActivateLens, DeactivateAllLenses, and ClearInstructions functions stay the same)
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
        tutorialInstructionText.text = "Lens switched!";
        Invoke("ClearInstructions", 2f);
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

    void ClearInstructions() => tutorialInstructionText.text = "";
}