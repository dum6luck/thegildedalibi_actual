using UnityEngine;
using TMPro;

public class InvestigationTutorial : MonoBehaviour
{
    [Header("UI References")]
    public GameObject magnifyingGlassUI; // The parent (Hand, Frame, Mask)
    public GameObject blueOverlay;       // The Blue Tint image child of the Mask
    public TextMeshProUGUI tutorialInstructionText;
    public Talking_Manager talkingManager;

    [Header("Lens System")]
    public GameObject blueLensCamera;

    private int tutorialStep = 0;

    void Start()
    {
        // Step 0: Ensure everything starts hidden
        magnifyingGlassUI.SetActive(false);
        if (blueOverlay != null) blueOverlay.SetActive(false);
        if (blueLensCamera != null) blueLensCamera.SetActive(false);

        tutorialInstructionText.text = "Press Q to take out magnifying glass.";
    }

    void Update()
    {
        switch (tutorialStep)
        {
            case 0: // Waiting for Q
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    magnifyingGlassUI.SetActive(true);
                    // The blueOverlay stays inactive even though the UI is now active
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
                    tutorialInstructionText.text = "Press 1 to change lenses.";
                    tutorialStep = 3;
                }
                break;

            case 3: // Waiting for 1
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    ActivateBlueLens();
                    tutorialInstructionText.text = "Clue detected!";
                    Invoke("ClearInstructions", 3f);
                    tutorialStep = 4;
                }
                break;
        }
    }

    void ActivateBlueLens()
    {
        // Turn on the Camera
        if (blueLensCamera != null)
            blueLensCamera.SetActive(true);

        // Turn on the Blue Tint inside the mask
        if (blueOverlay != null)
            blueOverlay.SetActive(true);
    }

    void ClearInstructions()
    {
        tutorialInstructionText.text = "";
    }
}