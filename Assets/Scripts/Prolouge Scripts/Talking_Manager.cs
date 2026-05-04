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

    [System.Serializable]
    public struct DialogueLine
    {
        public string characterName;
        [TextArea(3, 10)]
        public string sentence;
        public bool isItalic;
    }

    public List<DialogueLine> dialogueLines = new List<DialogueLine>();
    private int index = 0;
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private bool isFirstTimeTalking = false;
    private string currentInteractingNPC;

    private float lineStartTime;
    private float lastInputTime;
    private readonly float inputDelay = 0.15f;

    void Update()
    {
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
    public void SetCurrentNPC(string name)
    {
        currentInteractingNPC = name;
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
            dialogueDisplay.text = FormatText(dialogueLines[index].sentence, dialogueLines[index].isItalic);
            if (nextArrow != null) nextArrow.SetActive(true);
            return;
        }

        if (index < dialogueLines.Count - 1)
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

        string speaker = dialogueLines[index].characterName;
        if (nameDisplay != null) nameDisplay.text = speaker;

        // Switches camera based on speaker name
        SwitchCamera(speaker);

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(dialogueLines[index].sentence, dialogueLines[index].isItalic));
    }

    void SwitchCamera(string characterName)
    {
        // SAFETY: If you didn't drag the Cameras into the 'Camera Target Group' slot, 
        // this function stops immediately and won't mess up your other scenes.
        if (cameraTargetGroup == null)
        {
            return;
        }

        string speaker = characterName.ToUpper().Trim();

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

        foreach (char letter in fullText.ToCharArray())
        {
            currentText += letter;
            dialogueDisplay.text = FormatText(currentText, useItalics);
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        if (nextArrow != null) nextArrow.SetActive(true);
    }

    private void HandleDialogueEnd()
    {
        string lastLine = dialogueLines[index].sentence.Trim();

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

    void DisableManager()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        // NEW: Reset to Wide Shot when the dialogue ends
        ResetToWideShot();

        if (isFirstTimeTalking && mingleTracker != null)
        {
            mingleTracker.CheckProgression(currentInteractingNPC);
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
    }

    string FormatText(string text, bool useItalics) => useItalics ? $"<i>{text}</i>" : text;
}