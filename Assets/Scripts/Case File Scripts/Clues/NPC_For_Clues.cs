using System.Collections.Generic;
using UnityEngine;

/* 
 * SUMMARY:
 * This script is attached to physical objects in the world. 
 * It detects when the player is nearby and handles the 'E' key press 
 * to send data to the Case_File_UI.
 */

public class NPC_For_Clues : MonoBehaviour
{
    public NPCData npc;
    public string dialogue = "[Placeholder] You found anything yet?";

    /* 
     * Forms a dictionary of possible dialogues from
     * showcasing a clue with the clue's title as the key
     */
    [Header("Dictionary of Clue Dialogues")]
    public List<string> clue_keys = new List<string>();
    public List<string> dialogue_values = new List<string>();

    private bool is_player_nearby = false;
    private bool is_dialogue_ongoing = false;
    Dictionary<string, string> dialogue_dict = new Dictionary<string, string>();


    private void Start()
    {
        int dict_len = clue_keys.Count;
        if (clue_keys.Count > dialogue_values.Count) dict_len = dialogue_values.Count;

        for (int i = 0; i < dict_len; i++)
        {
            dialogue_dict[clue_keys[i]] = dialogue_values[i];
        }
    }

    private void Update()
    {
        if (is_player_nearby)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Talk();
            }
            else if (is_dialogue_ongoing) {
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
            // Trigger the VN-style dialogue box
            if (dialogue_manager != null && !dialogue_manager.Is_Dialogue_Ongoing(true))
            {
                is_dialogue_ongoing = true;
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
            }

            // Trigger the VN-style dialogue box
            if (dialogue_manager != null && ui_manager.Is_Showcasing())
            {
                string clue_name = ui_manager.Get_Showcased_Clue();
                is_dialogue_ongoing = false;
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) is_player_nearby = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) is_player_nearby = false;
    }
}