using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Interactable_Clue : MonoBehaviour
{
    public Clue_Data clue_info;

    [Header("Minigame Scene Settings")]
    [Tooltip("Check this if collecting this clue should trigger a scene switch.")]
    public bool loadsMinigameOnComplete = false;
    [Tooltip("Exact name of the scene to load after dialogue finishes.")]
    public string minigameSceneName = "CircuitMinigame";

    private bool is_player_nearby = false;
    private bool is_collected = false;
    private bool is_dialogue_ongoing = false;
    private LensSystem cameraLensSystem;

    private Case_File_UI ui_manager;
    private Dialogue_Manager dialogue_manager;

    private void Start()
    {
        // 1. RESTORE PLAYER POSITION: Check if a position was saved before loading the minigame
        if (PlayerPositionManager.HasSavedPosition)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // Disable CharacterController/NavMeshAgent if present to allow instant teleportation
                CharacterController controller = player.GetComponent<CharacterController>();
                if (controller != null) controller.enabled = false;

                player.transform.position = PlayerPositionManager.SavedPlayerPosition;
                player.transform.rotation = PlayerPositionManager.SavedPlayerRotation;

                if (controller != null) controller.enabled = true;

                // Clear so normal scene transitions don't force this position again
                PlayerPositionManager.ClearPosition();
            }
        }

        if (Camera.main != null)
        {
            cameraLensSystem = Camera.main.GetComponent<LensSystem>();
        }

        ui_manager = FindObjectOfType<Case_File_UI>();
        dialogue_manager = FindObjectOfType<Dialogue_Manager>();
    }

    private void Update()
    {
        if (!is_collected && is_player_nearby && Input.GetKeyDown(KeyCode.E))
        {
            this.Collect();
        }
    }

    private void Collect()
    {
        is_collected = true;

        if (ui_manager != null && clue_info != null)
        {
            ui_manager.Add_Clue_To_Log(clue_info);
        }

        if (cameraLensSystem != null)
        {
            cameraLensSystem.HideCluePrompt();
        }

        StartCoroutine(HandleDialogueSequence());
    }

    private IEnumerator HandleDialogueSequence()
    {
        // 1. Trigger Collection Dialogue
        if (dialogue_manager != null && clue_info != null && !string.IsNullOrEmpty(clue_info.collection_dialogue))
        {
            is_dialogue_ongoing = true;
            dialogue_manager.Show_Dialogue("DETECTIVE", clue_info.collection_dialogue);

            yield return null;

            bool dialogueFinished = false;

            while (!dialogueFinished)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (dialogue_manager.Is_Dialogue_Ongoing())
                    {
                        dialogue_manager.SendMessage("SkipTypewriter", SendMessageOptions.DontRequireReceiver);
                        dialogue_manager.SendMessage("Finish_Typing", SendMessageOptions.DontRequireReceiver);
                    }
                    else
                    {
                        dialogueFinished = true;
                    }
                }

                yield return null;
            }

            is_dialogue_ongoing = false;
        }

        // 2. Play Clue Cutscene (For normal clues)
        if (!loadsMinigameOnComplete && clue_info != null && clue_info.clue_cutscene != null)
        {
            if (dialogue_manager != null)
            {
                dialogue_manager.Show_Cutscene(clue_info.clue_cutscene);

                yield return null;

                while (!Input.GetMouseButtonDown(0))
                {
                    yield return null;
                }
            }
        }

        if (GetComponent<Collider>() != null)
        {
            GetComponent<Collider>().enabled = false;
        }

        // 3. Save player position & load Minigame Scene
        if (loadsMinigameOnComplete)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                PlayerPositionManager.SavePosition(player.transform);
            }

            Debug.Log("Dialogue fully read and dismissed. Saving position and loading minigame scene now.");
            SceneManager.LoadScene(minigameSceneName);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            is_player_nearby = true;

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

            if (cameraLensSystem != null)
            {
                cameraLensSystem.HideCluePrompt();
            }
        }
    }
}