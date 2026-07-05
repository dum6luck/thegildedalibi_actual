using UnityEngine;
using TMPro;

/* * SUMMARY:
 * Modifies the Alibi Investigation module to use keypress interaction.
 * Player presses 'E' to open the suspect's alibi text while in proximity.
 * Once open, the player can hover over and click the rich text link clue.
 */

public class AlibiInvestigation : MonoBehaviour
{
    [Header("NPC Settings")]
    public string npcName = "Suspect";

    [Header("Alibi Dialogue Setup")]
    [Tooltip("Write the full alibi text. Wrap your clue phrase in link tags.")]
    [TextArea(4, 6)]
    public string alibiText = "I was standing by the <link=\"tapestry\"><color=#FF3333><b>old tapestry</b></color></link> when the lights cut out.";

    [Tooltip("What the character says if the player clicks on the highlighted keyword clue.")]
    [TextArea(3, 5)]
    public string followUpResponse = "I noticed something strange near the base of the frame... like a hidden scrap of fiber.";

    [Header("UI Canvas Layout References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameTextField;
    public TextMeshProUGUI dialogueTextField;
    public GameObject pressFurtherButton;

    private bool isPlayerNearby = false;
    private bool dialogueUIActive = false;

    void Start()
    {
        if (pressFurtherButton != null) pressFurtherButton.SetActive(false);
    }

    void Update()
    {
        // 1. KEYPRESS DETECTION: Look for 'E' key while player is close to the character
        if (isPlayerNearby && !dialogueUIActive)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log($"[Alibi System] Player pressed E to interview: {npcName}");
                OpenAlibiDialogue();
            }
        }
        // 2. DIALOGUE INTERACTION PROCESSOR: If the panel is open, monitor mouse input for the clue words
        else if (dialogueUIActive && dialogueTextField != null)
        {
            // Unlock mouse cursor so the player can freely move it and click on text links
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (Input.GetMouseButtonDown(0))
            {
                CheckManualTextLinkClick();
            }
        }
    }

    void OpenAlibiDialogue()
    {
        if (dialoguePanel == null || dialogueTextField == null)
        {
            Debug.LogError($"[Alibi System] ERROR: Missing UI Panel or Text field assignment on {gameObject.name}!");
            return;
        }

        dialoguePanel.SetActive(true);
        dialogueUIActive = true;

        if (nameTextField != null) nameTextField.text = npcName;
        dialogueTextField.text = alibiText;

        if (pressFurtherButton != null) pressFurtherButton.SetActive(false);

        // Turn off character wandering while speaking
        NPCWander wander = GetComponent<NPCWander>();
        if (wander != null) wander.StopWandering();
    }

    void CheckManualTextLinkClick()
    {
        // Calculate intersection point between mouse screen position and the individual text link tag
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(dialogueTextField, Input.mousePosition, null);

        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = dialogueTextField.textInfo.linkInfo[linkIndex];
            Debug.Log($"[Alibi System] Detected click on custom keyword link: {linkInfo.GetLinkID()}");

            TriggerAskMoreOption();
        }
    }

    public void TriggerAskMoreOption()
    {
        if (dialogueTextField != null)
        {
            dialogueTextField.text = followUpResponse;
        }

        if (pressFurtherButton != null)
        {
            pressFurtherButton.SetActive(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            Debug.Log($"[Alibi System] Player entered range to press E for {npcName}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            dialogueUIActive = false;

            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            if (pressFurtherButton != null) pressFurtherButton.SetActive(false);

            NPCWander wander = GetComponent<NPCWander>();
            if (wander != null) wander.ResumeWandering();
        }
    }
}