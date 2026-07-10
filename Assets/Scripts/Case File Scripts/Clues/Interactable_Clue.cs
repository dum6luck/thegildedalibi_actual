using UnityEngine;

public class Interactable_Clue : MonoBehaviour
{
    public Clue_Data clue_info;
    private bool is_player_nearby = false;
    private bool is_collected = false;
    private bool is_dialogue_ongoing = false;
    private bool is_cutscene_ongoing = false;
    private LensSystem cameraLensSystem;

    private Case_File_UI ui_manager;
    private Dialogue_Manager dialogue_manager;

    private void Start()
    {
        // Automatically link up to the central lens controller on the Main Camera
        if (Camera.main != null)
        {
            cameraLensSystem = Camera.main.GetComponent<LensSystem>();
        }

        ui_manager = FindObjectOfType<Case_File_UI>();
        dialogue_manager = FindObjectOfType<Dialogue_Manager>();
    }

    private void Update()
    {
        if (is_dialogue_ongoing && dialogue_manager != null
                && !dialogue_manager.Is_Dialogue_Ongoing())
        {
            if (is_cutscene_ongoing || clue_info.clue_cutscene == null)
            {
                is_dialogue_ongoing = false;
                is_cutscene_ongoing = false;
            }
            else
            {
                is_cutscene_ongoing = true;
                dialogue_manager.Show_Cutscene(clue_info.clue_cutscene);
            }
        }
        else if (!is_collected && is_player_nearby && Input.GetKeyDown(KeyCode.E))
        {
            this.Collect();
        }
    }

    private void Collect()
    {
        //Case_File_UI ui_manager = FindObjectOfType<Case_File_UI>();
        //Dialogue_Manager dialogue_manager = FindObjectOfType<Dialogue_Manager>();

        is_collected = true;
        if (ui_manager != null)
        {
            ui_manager.Add_Clue_To_Log(clue_info);

            if (dialogue_manager != null && !string.IsNullOrEmpty(clue_info.collection_dialogue))
            {
                this.is_dialogue_ongoing = true;
                dialogue_manager.Show_Dialogue("DETECTIVE", clue_info.collection_dialogue);
            }

            // Clue is collected! Tell the camera to turn off the prompt immediately
            if (cameraLensSystem != null)
            {
                cameraLensSystem.HideCluePrompt();
            }

            if (GetComponent<Collider>() != null) GetComponent<Collider>().enabled = false;
            //this.enabled = false;
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