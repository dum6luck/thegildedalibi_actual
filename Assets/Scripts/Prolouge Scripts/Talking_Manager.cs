using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

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
    private string currentInteractingNPC; // Stores the name passed from NPCData

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

    // NEW: Call this from NPCData before starting the sequence to set the target NPC's name
    public void SetCurrentNPC(string name)
    {
        currentInteractingNPC = name;
    }

    public void StartDialogueSequence(bool isFirstTime)
    {
        this.gameObject.SetActive(true);

        isFirstTimeTalking = isFirstTime;
        index = 0;

        // Note: currentInteractingNPC should now be set via SetCurrentNPC() 
        // prior to this call to avoid counting the Detective.

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

        if (nameDisplay != null)
            nameDisplay.text = dialogueLines[index].characterName;

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(dialogueLines[index].sentence, dialogueLines[index].isItalic));
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

        if (isFirstTimeTalking && mingleTracker != null)
        {
            // Now uses the name specifically passed from the NPCData script
            mingleTracker.CheckProgression(currentInteractingNPC);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        this.gameObject.SetActive(false);
    }

    string FormatText(string text, bool useItalics) => useItalics ? $"<i>{text}</i>" : text;
}