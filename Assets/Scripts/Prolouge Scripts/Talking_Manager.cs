using UnityEngine;
using UnityEngine.UI; // Required for UI Image
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using Cinemachine;

public class Talking_Manager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI nameDisplay;
    public TextMeshProUGUI dialogueDisplay;
    public GameObject nextArrow;
    public GameObject dialoguePanel;
    [Tooltip("Assign your UI Background Image component here.")]
    public Image backgroundImage;

    [Header("Danganronpa Transition")]
    public UIFader uiFader;
    public MingleTracker mingleTracker;

    [Header("Danganronpa Camera")]
    [Tooltip("The parent object containing all your Virtual Cameras (Julian, Harlow, Detective, etc.)")]
    public Transform cameraTargetGroup;

    [Header("Audio")]
    [Tooltip("Assign an AudioSource here to play per-line sound effects.")]
    public AudioSource audioSource;

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

        [Tooltip("Optional - Assign a Sprite here to change the background image on this line.")]
        public Sprite backgroundImage;

        [Tooltip("Optional - plays when this line is displayed.")]
        public AudioClip soundEffect;

        [Tooltip("Check this if the sound effect should loop continuously across dialogue lines until explicitly stopped or replaced.")]
        public bool isLoopingSound;

        [Tooltip("Check this if you want to stop any currently playing looping audio on this line.")]
        public bool stopAudio;
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

        DialogueLine currentLine = dialogueLines[index];
        string speaker = currentLine.characterName;
        if (nameDisplay != null) nameDisplay.text = speaker;

        SwitchCamera(speaker);

        // --- Background Image Swapping ---
        if (currentLine.backgroundImage != null && backgroundImage != null)
        {
            backgroundImage.sprite = currentLine.backgroundImage;
        }

        // --- Audio Handling ---
        if (audioSource != null)
        {
            if (currentLine.stopAudio)
            {
                audioSource.Stop();
                audioSource.loop = false;
            }

            if (currentLine.soundEffect != null)
            {
                if (currentLine.isLoopingSound)
                {
                    audioSource.Stop();
                    audioSource.clip = currentLine.soundEffect;
                    audioSource.loop = true;
                    audioSource.Play();
                }
                else
                {
                    if (audioSource.loop)
                    {
                        audioSource.Stop();
                        audioSource.loop = false;
                    }
                    audioSource.PlayOneShot(currentLine.soundEffect);
                }
            }
        }

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(currentLine.sentence, currentLine.isItalic));
    }

    void SwitchCamera(string characterName)
    {
        if (cameraTargetGroup == null) return;

        string speaker = characterName.ToUpper().Trim();

        if (speaker == "WIDE")
        {
            ForceWideShot();
            return;
        }

        if (speaker == "DETECTIVE") return;

        foreach (Transform cam in cameraTargetGroup)
        {
            CinemachineVirtualCamera vcam = cam.GetComponent<CinemachineVirtualCamera>();
            if (vcam == null) continue;

            vcam.Priority = 10;
            string camName = cam.name.ToUpper();

            if (camName.Contains("WIDE"))
            {
                vcam.Priority = 15;
                continue;
            }

            if (speaker.Contains(camName.Replace("CAM_", "")) || camName.Contains(speaker))
            {
                vcam.Priority = 20;
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
        if (audioSource != null) audioSource.Stop();

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

    void OpenCaseFile()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        StartCoroutine(LoadCaseFile());
    }

    IEnumerator LoadCaseFile()
    {
        if (uiFader != null) uiFader.FadeOut();
        yield return new WaitForSeconds(0.8f);
        SceneManager.LoadScene(caseFileSceneName);
    }

    void DisableManager()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        ResetToWideShot();

        if (isFirstTimeTalking && mingleTracker != null)
        {
            mingleTracker.CheckProgression(currentInteractingNPC);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        this.gameObject.SetActive(false);
    }

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
                vcam.Priority = 25;
            }
            else
            {
                vcam.Priority = 10;
            }
        }
    }

    string FormatText(string text, bool useItalics) => useItalics ? $"<i>{text}</i>" : text;
}