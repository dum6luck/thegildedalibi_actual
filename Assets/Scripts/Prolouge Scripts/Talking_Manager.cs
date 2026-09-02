using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using Cinemachine; // Correct namespace for Unity 2021

public class Talking_Manager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI nameDisplay;
    public TextMeshProUGUI dialogueDisplay;
    public GameObject nextArrow;
    public GameObject dialoguePanel;
    public AudioSource audioSource;
    public List<AudioClip> default_voice_samples;

    [Header("Danganronpa Transition")]
    public UIFader uiFader;
    public MingleTracker mingleTracker;

    [Header("Danganronpa Camera")]
    [Tooltip("The parent object containing all your Virtual Cameras (Julian, Harlow, Detective, etc.)")]
    public Transform cameraTargetGroup;

    [Header("Settings")]
    [Range(0.01f, 0.1f)]
    public float typingSpeed = 0.05f;

    [Header("Reusable Scene Transition")]
    public string triggerSentence;
    public string sceneToLoad;

    [Header("Case File Scene")]
    public string caseFileSceneName = "CaseFile";

    [System.Serializable]
    public struct DialogueLine
    {
        public string characterName;
        [TextArea(3, 10)]
        public string sentence;
        public bool isItalic;
    }

    public OverworldCutsceneData dialogueLines;
    private int index = 0;
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private bool isFirstTimeTalking = false;
    private Character_Data currentInteractingNPC;
    private SpriteRenderer currentInteractingSprite;

    private float lineStartTime;
    private float lastInputTime;
    private readonly float inputDelay = 0.15f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            OpenCaseFile();
            return;
        }

        if (dialoguePanel != null && dialoguePanel.activeSelf)
        {
            if (Time.time - lineStartTime < 0.15f) return;

            if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) && Time.time - lastInputTime > inputDelay)
            {
                lastInputTime = Time.time;
                AdvanceDialogue();
            }
        }
    }

    // Call this from NPCData before starting the sequence
    public void SetCurrentNPC(Character_Data npc, SpriteRenderer sprite)
    {
        currentInteractingNPC = npc;
        currentInteractingSprite = sprite;
    }

    public void StartDialogueSequence(bool isFirstTime)
    {
        this.gameObject.SetActive(true);
        isFirstTimeTalking = isFirstTime;
        index = 0;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
            if (uiFader != null) uiFader.FadeIn();
        }

        DisplayLine();
    }

    public void AdvanceDialogue()
    {
        if (isTyping)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            isTyping = false;
            dialogueDisplay.text = FormatText(dialogueLines.frames[index].dialogueLine, dialogueLines.frames[index].isItalic);
            if (nextArrow != null) nextArrow.SetActive(true);
            return;
        }

        if (index < dialogueLines.frames.Count - 1)
        {
            index++;
            DisplayLine();
        }
        else
        {
            HandleDialogueEnd();
        }
    }

    void DisplayLine()
    {
        lineStartTime = Time.time;
        if (nextArrow != null) nextArrow.SetActive(false);

        Character_Data npc = dialogueLines.frames[index].speaker;
        string speaker = npc == null ? "" : npc.name;
        if (nameDisplay != null) nameDisplay.text = speaker;

        if (currentInteractingSprite != null && currentInteractingNPC != null) currentInteractingSprite.sprite = currentInteractingNPC.Get_Overworld_Sprite(dialogueLines.frames[index].emotion);

        // Switches camera based on speaker name
        SwitchCamera(npc);

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(dialogueLines.frames[index].dialogueLine, dialogueLines.frames[index].isItalic));
    }

    void SwitchCamera(Character_Data npc)
    {
        // SAFETY: If you didn't drag the Cameras into the 'Camera Target Group' slot, 
        // this function stops immediately and won't mess up your other scenes.
        if (cameraTargetGroup == null)
        {
            return;
        }

        string speaker = npc == null ? "".ToUpper().Trim() : npc.name.ToUpper().Trim();

        // Reset to wide shot logic if keyword is used
        if (speaker == "WIDE")
        {
            ForceWideShot();
            return;
        }

        // Keep the current camera if the Detective is talking
        if (speaker == "DETECTIVE")
        {
            return;
        }

        bool foundMatch = false;

        foreach (Transform cam in cameraTargetGroup)
        {
            CinemachineVirtualCamera vcam = cam.GetComponent<CinemachineVirtualCamera>();
            if (vcam == null) continue;

            // Reset NPC cameras to 10
            vcam.Priority = 10;

            string camName = cam.name.ToUpper();

            // Ensure the Wide shot stays as the baseline
            if (camName.Contains("WIDE"))
            {
                vcam.Priority = 15;
                continue;
            }

            // Match speaker to camera
            if (speaker.Contains(camName.Replace("CAM_", "")) || camName.Contains(speaker))
            {
                CameraHelper camDetails = cam.GetComponent<CameraHelper>();
                if (camDetails != null && npc != null)
                {
                    camDetails.sprite.sprite = npc.Get_Overworld_Sprite(dialogueLines.frames[index].emotion);
                }
                vcam.Priority = 20;
                foundMatch = true;
            }
        }
    }

    void ForceWideShot()
    {
        foreach (Transform cam in cameraTargetGroup)
        {
            CinemachineVirtualCamera vcam = cam.GetComponent<CinemachineVirtualCamera>();
            if (vcam == null) continue;
            vcam.Priority = (cam.name.ToUpper().Contains("WIDE")) ? 20 : 10;
        }
    }

    IEnumerator TypeText(string fullText, bool useItalics)
    {
        isTyping = true;
        dialogueDisplay.text = "";
        string currentText = "";
        int tick = 0;

        foreach (char letter in fullText.ToCharArray())
        {
            currentText += letter;
            dialogueDisplay.text = FormatText(currentText, useItalics);

            // Prevents loud echoing for debug purposes
            if ((tick % 20) - 1 == 0) audioSource.Stop();

            PlaySound();
            tick++;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        if (nextArrow != null) nextArrow.SetActive(true);
    }

    private void HandleDialogueEnd()
    {
        string lastLine = dialogueLines.frames[index].dialogueLine.Trim();

        if (!string.IsNullOrEmpty(triggerSentence) && lastLine == triggerSentence.Trim())
        {
            if (uiFader != null) { uiFader.FadeOut(); Invoke(nameof(LoadNextScene), 0.8f); }
            else LoadNextScene();
        }
        else
        {
            if (uiFader != null) { uiFader.FadeOut(); Invoke(nameof(DisableManager), 0.6f); }
            else DisableManager();
        }
    }

    void LoadNextScene() => SceneManager.LoadScene(sceneToLoad);

    void OpenCaseFile()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(caseFileSceneName);
    }

    void DisableManager()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        // NEW: Reset to Wide Shot when the dialogue ends
        ResetToWideShot();

        if (isFirstTimeTalking && mingleTracker != null)
        {
            mingleTracker.CheckProgression(currentInteractingNPC.name);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        this.gameObject.SetActive(false);
    }

    // Helper function to force the Wide Shot to take priority again
    void ResetToWideShot()
    {
        if (cameraTargetGroup == null) return;

        foreach (Transform cam in cameraTargetGroup)
        {
            CinemachineVirtualCamera vcam = cam.GetComponent<CinemachineVirtualCamera>();
            if (vcam == null) continue;

            string camName = cam.name.ToUpper();

            if (camName.Contains("WIDE"))
            {
                vcam.Priority = 25; // Set higher than NPC (20) and previous Wide (15)
            }
            else
            {
                vcam.Priority = 10;
            }
        }

        IEnumerator LoadCaseFile()
        {
            if (uiFader != null)
                uiFader.FadeOut();

            yield return new WaitForSeconds(0.8f);
            caseFileSceneName = "Case_File_Scene";

            SceneManager.LoadScene(caseFileSceneName);
        }
    }

    private void PlaySound()
    {
        //Play sounds only if the source exists
        if (audioSource == null) return;


        if (dialogueLines != null)
        {
            Character_Data npc = dialogueLines.frames[index].speaker;

            if (npc != null)
            {
                //Stop the audioSource so that the new sentence does not overlap with the old one
                //audioSource.Stop();

                //Play sentence sound
                if (npc.voiceSamples != null)
                {
                    audioSource.PlayOneShot(
                        npc.voiceSamples[UnityEngine.Random.Range(0, npc.voiceSamples.Count)]);
                }

                return;
            }
        }

        if (default_voice_samples != null)
        {
            audioSource.PlayOneShot(
                default_voice_samples[UnityEngine.Random.Range(0, default_voice_samples.Count)]);
        }
    }

    string FormatText(string text, bool useItalics) => useItalics ? $"<i>{text}</i>" : text;
}