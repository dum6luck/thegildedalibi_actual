using System.Collections.Generic;
using UnityEngine;

/* * SUMMARY:
 * This script handles NPC dialogue and clues.
 * It has been upgraded to aggressively look up and down the object hierarchy
 * to find the NPCWander script and force it to a dead stop.
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
        // FIX: Look on this object, and if it's not here, check parent objects too!
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
        Dialogue_Manager dialogue_manager = FindObjectOfType<Dialogue_Manager>();

        // If dialogue was running but is now closed, let them walk again
        if (is_dialogue_ongoing && dialogue_manager != null && !dialogue_manager.Is_Dialogue_Ongoing(true))
        {
            is_dialogue_ongoing = false;
            if (wanderScript != null)
            {
                wanderScript.ResumeWandering();
            }
        }

        if (is_player_nearby)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Talk();
            }
            else if (is_dialogue_ongoing)
            {
                Showcase();
            }
        }
    }

    private void Talk()
    {
        Case_File_UI ui_manager = FindObjectOfType<Case_File_UI>();
        Dialogue_Manager dialogue_manager = FindObjectOfType<Dialogue_Manager>();

        if (ui_manager != null)
        {
            if (dialogue_manager != null && !dialogue_manager.Is_Dialogue_Ongoing(true))
            {
                is_dialogue_ongoing = true;

                // Double check layout linkage right before freezing
                if (wanderScript == null) FindWanderScriptFallback();

                if (wanderScript != null)
                {
                    wanderScript.StopWandering();
                }

                dialogue_manager.Show_Dialogue(npc.npcName, dialogue, true);
            }
        }
    }

    private void Showcase()
    {
        Case_File_UI ui_manager = FindObjectOfType<Case_File_UI>();
        Dialogue_Manager dialogue_manager = FindObjectOfType<Dialogue_Manager>();

        if (ui_manager != null)
        {
            if (!dialogue_manager.Is_Dialogue_Ongoing(true))
            {
                is_dialogue_ongoing = false;
                if (wanderScript != null) wanderScript.ResumeWandering();
            }

            if (dialogue_manager != null && ui_manager.Is_Showcasing())
            {
                string clue_name = ui_manager.Get_Showcased_Clue();
                is_dialogue_ongoing = false;

                if (wanderScript == null) FindWanderScriptFallback();
                if (wanderScript != null) wanderScript.StopWandering();

                try
                {
                    dialogue_manager.Show_Dialogue(npc.npcName, dialogue_dict[clue_name]);
                }
                catch
                {
                    dialogue_manager.Show_Dialogue(npc.npcName, "[Placeholder] It seems like this person doesn't want to talk.");
                }
            }
        }
    }

    // Emergency manual sweep loop if assignments get unlinked at runtime
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