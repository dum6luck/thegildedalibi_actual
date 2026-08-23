using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Interactable_Clue : MonoBehaviour
{
    public Clue_Data clue_info;

    [Header("Minigame Scene Settings")]
    public bool loadsMinigameOnComplete = false;
    public string minigameSceneName = "CircuitMinigame";

    [Header("Post-Minigame Cutscene")]
    public Cutscene_Data postMinigameCutscene;
    public float cutsceneDelay = 1.0f; // seconds to wait before cutscene appears

    // Static queue stores cutscene data across scene transitions
    public static Cutscene_Data QueuedCutscene = null;

    private bool is_player_nearby = false;
    private bool is_collected = false;
    private LensSystem cameraLensSystem;
    private Case_File_UI ui_manager;
    private Dialogue_Manager dialogue_manager;

    private IEnumerator Start()
    {
        // 1. Wait one frame so managers finish their Awake() on scene load
        yield return null;

        // 2. RESTORE PLAYER POSITION (immediate, no delay)
        if (PlayerPositionManager.HasSavedPosition)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                CharacterController controller = player.GetComponent<CharacterController>();
                if (controller != null) controller.enabled = false;

                player.transform.position = PlayerPositionManager.SavedPlayerPosition;
                player.transform.rotation = PlayerPositionManager.SavedPlayerRotation;

                if (controller != null) controller.enabled = true;

                PlayerPositionManager.ClearPosition();
            }
        }

        // Grab references early so they're ready regardless of cutscene delay
        if (Camera.main != null)
        {
            cameraLensSystem = Camera.main.GetComponent<LensSystem>();
        }

        ui_manager = FindObjectOfType<Case_File_UI>();
        dialogue_manager = FindObjectOfType<Dialogue_Manager>();

        // 3. PLAY QUEUED CUTSCENE (after a short delay), through Dialogue_Manager
        //    so it matches the look/feel of your other cutscenes
        if (QueuedCutscene != null)
        {
            Cutscene_Data cutsceneToPlay = QueuedCutscene;
            QueuedCutscene = null; // Clear queue so it doesn't repeat/re-trigger

            yield return new WaitForSeconds(cutsceneDelay);

            if (Dialogue_Manager.Instance != null)
            {
                Dialogue_Manager.Instance.Show_Cutscene(cutsceneToPlay);
            }
        }
    }

    private void Update()
    {
        if (!is_collected && is_player_nearby && Input.GetKeyDown(KeyCode.E))
        {
            Collect();
        }
    }

    /// <summary>
    /// Collects the clue and logs it to the Case File UI. Public so UnityEvents can trigger it.
    /// </summary>
    public void Collect()
    {
        if (is_collected) return;

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
        if (dialogue_manager != null && clue_info != null && !string.IsNullOrEmpty(clue_info.collection_dialogue))
        {
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
        }

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

        // SAVE POSITION & QUEUE CUTSCENE BEFORE LOADING MINIGAME
        if (loadsMinigameOnComplete)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                PlayerPositionManager.SavePosition(player.transform);
            }

            if (postMinigameCutscene != null)
            {
                QueuedCutscene = postMinigameCutscene;
            }

            SceneManager.LoadScene(minigameSceneName);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            is_player_nearby = true;
            if (cameraLensSystem != null) cameraLensSystem.ShowCluePrompt();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            is_player_nearby = false;
            if (cameraLensSystem != null) cameraLensSystem.HideCluePrompt();
        }
    }
}