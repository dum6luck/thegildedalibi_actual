using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* * SUMMARY:
 * This script handles NPC dialogue and clues.
 * Upgraded so Julian's unique mask response ONLY triggers if the player
 * investigated the mask using the Blue or Red lens�not the Blacklight!
 */

public class NPC_For_Clues : MonoBehaviour
{
    public Character_Data npc;
    public OverworldDialogueData dialogue;

    [Header("Specific Mask Interaction Setup")]
    public string maskClueID = "Mask"; // Matches the 'clue_title' in Case_File_UI
    public string maskInvestigatedResponse = "You didn't find anything on the mask? I thought there was something...";

    private bool hasDeliveredMaskReaction = false;

    [Header("Dictionary of Clue Dialogues")]
    public List<string> clue_keys = new List<string>();
    public List<OverworldDialogueData> dialogue_values = new List<OverworldDialogueData>();

    private bool is_player_nearby = false;
    private bool is_dialogue_ongoing = false;
    private SpriteRenderer sprite;
    Dictionary<string, OverworldDialogueData> dialogue_dict = new Dictionary<string, OverworldDialogueData>();

    private NPCWander wanderScript;

    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        wanderScript = GetComponent<NPCWander>();
        if (wanderScript == null)
        {
            wanderScript = GetComponentInParent<NPCWander>();
        }

        int dict_len = clue_keys.Count;
        if (clue_keys.Count > dialogue_values.Count) dict_len = dialogue_values.Count;

        for (int i = 0; i < dict_len; i++)
        {
            dialogue_dict[clue_keys[i]] = dialogue_values[i];
        }
    }

    private void Update()
    {
        Case_File_UI ui_manager = FindObjectOfType<Case_File_UI>();
        Dialogue_Manager dialogue_manager = FindObjectOfType<Dialogue_Manager>();

        if (is_player_nearby && ui_manager != null && ui_manager.Is_Showcasing())
        {
            HandleShowcaseDialogue(ui_manager, dialogue_manager);
            return;
        }

        if (is_dialogue_ongoing && dialogue_manager != null && !dialogue_manager.Is_Dialogue_Ongoing(true))
        {
            is_dialogue_ongoing = false;
            if (wanderScript != null)
            {
                wanderScript.ResumeWandering();
            }
        }

        if (is_player_nearby && !is_dialogue_ongoing)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Talk(dialogue_manager);
            }
        }
    }

    private void Talk(Dialogue_Manager dialogue_manager)
    {
        if (dialogue_manager != null && !dialogue_manager.Is_Dialogue_Ongoing(true))
        {
            is_dialogue_ongoing = true;

            if (wanderScript == null) FindWanderScriptFallback();
            if (wanderScript != null) wanderScript.StopWandering();

            // --- NARRATIVE PROGRESSION BRAIN WITH LENS CHECK ---
            bool hasPlayerInvestigatedMask = PlayerHasMaskClue();
            bool structuralLensValid = IsValidLensActive();

            // ONLY trigger Julian's unique prompt if it's Julian, they have the mask clue, 
            // a valid lens (Blue/Red) is active, and they haven't heard it yet.
            if (npc.name.ToUpper() == "JULIAN" && hasPlayerInvestigatedMask && structuralLensValid && !hasDeliveredMaskReaction)
            {
                dialogue_manager.Show_Dialogue(npc, maskInvestigatedResponse, true);
                hasDeliveredMaskReaction = true;
                Debug.Log("Narrative Flow: Delivered Julian's specific mask line (Validated Blue/Red Lens).");
            }
            else
            {
                // Defaults directly to "You found anything yet?" if Blacklight or no lens is active
                if (dialogue != null)
                {
                    dialogue_manager.Display_Dialogue(npc, dialogue, sprite, true);
                }
                else
                {
                    dialogue_manager.Show_Dialogue(npc, "[Placeholder] You found anything yet?", true);
                }
            }
        }
    }

    private bool PlayerHasMaskClue()
    {
        Case_File_UI ui_manager = FindObjectOfType<Case_File_UI>();
        if (ui_manager != null)
        {
            return ui_manager.Has_Collected_Clue(maskClueID);
        }
        return false;
    }

    private bool IsValidLensActive()
    {
        if (LensDataCarrier.Instance != null)
        {
            string currentLens = LensDataCarrier.Instance.activeLensLayerName;
            return (currentLens == "Blue Light" || currentLens == "Red Light");
        }
        return false;
    }

    private void HandleShowcaseDialogue(Case_File_UI ui_manager, Dialogue_Manager dialogue_manager)
    {
        string clue_name = ui_manager.Get_Showcased_Clue();
        is_dialogue_ongoing = true;

        if (wanderScript == null) FindWanderScriptFallback();
        if (wanderScript != null) wanderScript.StopWandering();

        if (dialogue_manager != null)
        {
            if (dialogue_dict.ContainsKey(clue_name) && dialogue_dict[clue_name].frames.Count > 0)
            {
                dialogue_manager.Display_Dialogue(npc, dialogue_dict[clue_name], sprite, true);
            }
            else
            {
                dialogue_manager.Show_Dialogue(npc, "[Placeholder] I don't know anything about that item.", true);
            }
        }
    }

    private void FindWanderScriptFallback()
    {
        wanderScript = GetComponent<NPCWander>();
        if (wanderScript == null) wanderScript = GetComponentInParent<NPCWander>();
        if (wanderScript == null) wanderScript = GetComponentInChildren<NPCWander>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) is_player_nearby = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            is_player_nearby = false;
            is_dialogue_ongoing = false;
            if (wanderScript != null) wanderScript.ResumeWandering();
        }
    }
}