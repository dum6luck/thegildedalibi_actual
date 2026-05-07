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
    private bool is_player_nearby = false;

    private void Update()
    {
        if (is_player_nearby && Input.GetKeyDown(KeyCode.E))
        {
            Talk();
        }
    }

    private void Talk()
    {
        Case_File_UI ui_manager = FindObjectOfType<Case_File_UI>();
        Dialogue_Manager dialogue_manager = FindObjectOfType<Dialogue_Manager>();

        if (ui_manager != null)
        {
            // Trigger the VN-style dialogue box
            if (dialogue_manager != null)
            {
                dialogue_manager.Show_Dialogue(npc.npcName, "You found anything yet?", true);
            }

            // GetComponent<Collider>().enabled = false;
            // this.enabled = false;
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