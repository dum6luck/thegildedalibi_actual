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
    [Tooltip("If the last line matches this EXACTLY, the scene will change.")]
    public string triggerSentence;
    [Tooltip("The name of the scene to load.")]
    public string sceneToLoad;

    [System.Serializable]
    public struct DialogueLine
    {
        public string characterName;
        [TextArea(3, 10)]
        public string sentence;
        public bool isItalic;
    }

    public List<DialogueLine> dialogueLines;
    private int index = 0;
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private bool isFirstTimeTalking = false;

    // --- ADDED UPDATE LOOP FOR CLICKS ---
    void Update()
    {
        // If the dialogue is active, allow clicking or pressing Space/Enter to advance
        if (dialoguePanel != null && dialoguePanel.activeSelf)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                AdvanceDialogue();
            }
        }
    }

    public void StartDialogueSequence(bool isFirstTime)
    {
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
            StopCoroutine(typingCoroutine);
            isTyping = false;
            dialogueDisplay.text = FormatText(dialogueLines[index].sentence, dialogueLines[index].isItalic);
            if (nextArrow != null) nextArrow.SetActive(true);
            return;
        }

        if (index < dialogueLines.Count - 1)
        {
            index++;
            if (nextArrow != null) nextArrow.SetActive(false);
            DisplayLine();
        }
        else
        {
            string lastLine = dialogueLines[index].sentence.Trim();

            if (!string.IsNullOrEmpty(triggerSentence) && lastLine == triggerSentence.Trim())
            {
                if (uiFader != null)
                {
                    uiFader.FadeOut();
                    Invoke("LoadNextScene", 0.8f);
                }
                else
                {
                    LoadNextScene();
                }
            }
            else
            {
                // This is where "Hmm... Not seeing anything" finishes
                if (uiFader != null)
                {
                    uiFader.FadeOut();
                    Invoke("DisableManager", 0.6f);
                }
                else
                {
                    DisableManager();
                }
            }
        }
    }

    void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
            SceneManager.LoadScene(sceneToLoad);
    }

    void DisplayLine()
    {
        if (dialogueLines.Count == 0) return;

        if (nameDisplay != null)
            nameDisplay.text = dialogueLines[index].characterName;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(dialogueLines[index].sentence, dialogueLines[index].isItalic));
    }

    IEnumerator TypeText(string fullText, bool useItalics)
    {
        isTyping = true;
        dialogueDisplay.text = "";
        string currentDisplayedText = "";

        foreach (char letter in fullText.ToCharArray())
        {
            currentDisplayedText += letter;
            dialogueDisplay.text = FormatText(currentDisplayedText, useItalics);
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        if (nextArrow != null) nextArrow.SetActive(true);
    }

    // --- REFINED DISABLE MANAGER ---
    void DisableManager()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        // Safety check for other scenes
        if (isFirstTimeTalking && mingleTracker != null)
        {
            mingleTracker.CheckProgression();
        }

        // Hide and lock the cursor so it doesn't stay on screen
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Turn off the manager so the tutorial script continues
        this.gameObject.SetActive(false);
    }

    string FormatText(string text, bool useItalics)
    {
        return useItalics ? $"<i>{text}</i>" : text;
    }
}