using UnityEngine;

/* 
 * SUMMARY:
 * This script is attached to physical objects in the world. 
 * It detects when the player is nearby and handles the 'E' key press 
 * to send data to the Case_File_UI.
 */

public class Interactable_Clue : MonoBehaviour
{
    public Clue_Data clue_info;
    private bool is_player_nearby = false;

    private void Update()
    {
        if (is_player_nearby && Input.GetKeyDown(KeyCode.E))
        {
            Collect();
        }
    }

    private void Collect()
    {
        Case_File_UI ui_manager = FindObjectOfType<Case_File_UI>();
        Dialogue_Manager dialogue_manager = FindObjectOfType<Dialogue_Manager>();

        if (ui_manager != null)
        {
            // Add to the notebook
            ui_manager.Add_Clue_To_Log(clue_info);

            // Trigger the VN-style dialogue box
            if (dialogue_manager != null && !string.IsNullOrEmpty(clue_info.collection_dialogue))
            {
                dialogue_manager.Show_Dialogue("DETECTIVE", clue_info.collection_dialogue);
            }

            // REMOVE OR COMMENT OUT THIS LINE:
            // gameObject.SetActive(false); 

            // OPTIONAL: If you want to prevent the player from "collecting" it 
            // multiple times, you can disable the script or collider instead:
            GetComponent<Collider>().enabled = false;
            this.enabled = false;
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