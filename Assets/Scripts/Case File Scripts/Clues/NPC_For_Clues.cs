using System.Collections.Generic;
using UnityEngine;

/* * SUMMARY:
 * This script handles NPC dialogue and clues.
 * Upgraded so that showcasing a clue instantly triggers the custom response dialogue
 * without forcing the player to press 'E' again.
 */

public class NPC_For_Clues : MonoBehaviour
{
    public NPCData npc;
    public string dialogue = "[Placeholder] You found anything yet?";

    [Header("Dictionary of Clue Dialogues")]
    public List<string> clue_keys = new List<string>();
    public List<string> dialogue_values = new List<string>();

    private bool is_player_nearby = false;
    private bool is_dialogue_ongoing = false;
    Dictionary<string, string> dialogue_dict = new Dictionary<string, string>();

    private NPCWander wanderScript;

    private void Start()
    {
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

        // 1. CONSTANT WATCHER: If the player just selected a clue to showcase to THIS nearby NPC, intercept it immediately!
        if (is_player_nearby && ui_manager != null && ui_manager.Is_Showcasing())
        {
            HandleShowcaseDialogue(ui_manager, dialogue_manager);
            return;
        }

        // 2. Clear conversation state and let the NPC walk again if dialogue finishes normally
        if (is_dialogue_ongoing && dialogue_manager != null && !dialogue_manager.Is_Dialogue_Ongoing(true))
        {
            is_dialogue_ongoing = false;
            if (wanderScript != null)
            {
                wanderScript.ResumeWandering();
            }
        }

        // 3. Regular interaction triggers
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

            dialogue_manager.Show_Dialogue(npc.npcName, dialogue, true);
        }
    }

    private void HandleShowcaseDialogue(Case_File_UI ui_manager, Dialogue_Manager dialogue_manager)
    {
        string clue_name = ui_manager.Get_Showcased_Clue();
        is_dialogue_ongoing = true;

        if (wanderScript == null) FindWanderScriptFallback();
        if (wanderScript != null) wanderScript.StopWandering();

        if (dialogue_manager != null)
        {
            if (dialogue_dict.ContainsKey(clue_name))
            {
                dialogue_manager.Show_Dialogue(npc.npcName, dialogue_dict[clue_name], true);
            }
            else
            {
                // Fallback default description line if the specific character doesn't have unique dialogue for this clue asset
                dialogue_manager.Show_Dialogue(npc.npcName, "[Placeholder] I don't know anything about that item.", true);
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