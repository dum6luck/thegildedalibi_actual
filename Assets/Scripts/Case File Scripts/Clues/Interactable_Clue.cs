using UnityEngine;

public class Interactable_Clue : MonoBehaviour
{
    public Clue_Data clue_info;
    private bool is_player_nearby = false;
    private LensSystem cameraLensSystem;

    private void Start()
    {
        // Automatically link up to the central lens controller on the Main Camera
        if (Camera.main != null)
        {
            cameraLensSystem = Camera.main.GetComponent<LensSystem>();
        }
    }

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
            ui_manager.Add_Clue_To_Log(clue_info);

            if (dialogue_manager != null && !string.IsNullOrEmpty(clue_info.collection_dialogue))
            {
                dialogue_manager.Show_Dialogue("DETECTIVE", clue_info.collection_dialogue);
            }

            // Clue is collected! Tell the camera to turn off the prompt immediately
            if (cameraLensSystem != null)
            {
                cameraLensSystem.HideCluePrompt();
            }

            if (GetComponent<Collider>() != null) GetComponent<Collider>().enabled = false;
            this.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            is_player_nearby = true;

            // Tell the screen UI to turn on
            if (cameraLensSystem != null)
            {
                cameraLensSystem.ShowCluePrompt();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            is_player_nearby = false;

            // Tell the screen UI to turn off
            if (cameraLensSystem != null)
            {
                cameraLensSystem.HideCluePrompt();
            }
        }
    }
}