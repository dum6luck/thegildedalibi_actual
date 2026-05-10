using UnityEngine;
using System.Collections.Generic;

public class NPC_Interrogatable : MonoBehaviour
{
    public string character_name;
    public GameObject interrogation_menu_panel; // A new UI panel with a vertical layout group
    public GameObject dialogue_button_prefab;   // A simple button prefab

    public void OnInteract()
    {
        Open_Interrogation_Menu();
    }

    void Open_Interrogation_Menu()
    {
        interrogation_menu_panel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Clear old buttons
        foreach (Transform child in interrogation_menu_panel.transform)
        {
            Destroy(child.gameObject);
        }

        // Get the list of clues we actually have from the Case File
        // (Assuming your Case_File_UI has a way to track the list of Clue_Data)
        List<Clue_Data> collected_clues = FindObjectOfType<Case_File_UI>().Get_Collected_Clues();

        foreach (Clue_Data clue in collected_clues)
        {
            GameObject btnObj = Instantiate(dialogue_button_prefab, interrogation_menu_panel.transform);
            btnObj.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Talk about " + clue.clue_title;

            btnObj.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => {
                Talk_About_Clue(clue);
            });
        }
    }

    void Talk_About_Clue(Clue_Data clue)
    {
        string reaction = "I don't know anything about that.";

        // Find this specific NPC's reaction in the Clue_Data
        foreach (var r in clue.npc_reactions)
        {
            if (r.npc_name == character_name)
            {
                reaction = r.reaction_text;
                break;
            }
        }

        interrogation_menu_panel.SetActive(false);
        FindObjectOfType<Dialogue_Manager>().Show_Dialogue(character_name, reaction);
    }
}