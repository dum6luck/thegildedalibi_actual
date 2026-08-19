using HeneGames.DialogueSystem;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BreakerSequenceHandler : MonoBehaviour
{
    [Header("Minigame Scene Settings")]
    [Tooltip("Exact name of the circuit minigame scene in Build Settings.")]
    public string minigameSceneName = "CircuitMinigame";

    [Header("Cutscene Settings")]
    [Tooltip("The EXACT cutscene asset to play after returning from the solved puzzle.")]
    public Cutscene_Data postMinigameCutscene;

    [Header("Input Settings")]
    public KeyCode interactKey = KeyCode.E;

    // Static queue to preserve the intended cutscene across scene loads
    public static Cutscene_Data QueuedCutscene = null;

    private bool isPlayerInTrigger = false;
    private bool sequenceStarted = false;
    private Interactable_Clue attachedClue;

    private void Awake()
    {
        attachedClue = GetComponent<Interactable_Clue>();
    }

    private void Start()
    {
        // 1. RETURN FROM MINIGAME: Check if a cutscene was queued
        if (QueuedCutscene != null)
        {
            Cutscene_Data cutsceneToPlay = QueuedCutscene;
            QueuedCutscene = null;

            if (CutsceneController.Instance != null)
            {
                CutsceneController.Instance.cutsceneData = cutsceneToPlay;
                CutsceneController.Instance.DisplayFrame(0);
            }
        }
    }

    private void Update()
    {
        if (isPlayerInTrigger && Input.GetKeyDown(interactKey) && !sequenceStarted)
        {
            StartCoroutine(PlayDialogueThenLoadMinigame());
        }
    }

    private IEnumerator PlayDialogueThenLoadMinigame()
    {
        sequenceStarted = true;

        // Trigger clue collection & dialogue
        if (attachedClue != null)
        {
            attachedClue.SendMessage("Collect", SendMessageOptions.DontRequireReceiver);
            attachedClue.SendMessage("Interact", SendMessageOptions.DontRequireReceiver);
        }

        // Wait a small delay for HeneGames DialogueUI to open and set IsProcessingDialogue to true
        yield return new WaitForSeconds(0.2f);

        // Wait until player clicks/finishes reading and DialogueUI completes and closes
        if (DialogueUI.instance != null)
        {
            while (DialogueUI.instance.IsProcessingDialogue())
            {
                yield return null;
            }
        }

        // Store post-minigame cutscene
        QueuedCutscene = postMinigameCutscene;

        // Load the minigame scene ONLY after dialogue box is fully dismissed
        SceneManager.LoadScene(minigameSceneName);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerInTrigger = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerInTrigger = false;
    }
}