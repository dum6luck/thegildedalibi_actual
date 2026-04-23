using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement; // Added for scene switching

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

    public void StartDialogueSequence(bool isFirstTime)
    {
        isFirstTimeTalking = isFirstTime;
        index = 0;

        if (uiFader != null)
        {
            dialoguePanel.SetActive(true);
            uiFader.FadeIn();
        }
        else if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
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
            // --- SCENE TRIGGER CHECK ---
            // We check if the line that just finished is your specific trigger line
            string lastLine = dialogueLines[index].sentence;
            string triggerLine = "Everyone in this room just got some kind of bad news. And now we're all supposed to have a nice time together. Great party, Max.";

            if (lastLine.Trim() == triggerLine.Trim())
            {
                if (uiFader != null)
                {
                    uiFader.FadeOut();
                    Invoke("LoadMingleScene", 0.8f);
                }
                else
                {
                    LoadMingleScene();
                }
            }
            else
            {
                // Normal behavior: just close the dialogue
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

    void LoadMingleScene()
    {
        // Make sure "MingleScene" matches the name in your Build Settings exactly!
        SceneManager.LoadScene("Act1_1");
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

    void DisableManager()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        if (isFirstTimeTalking && mingleTracker != null)
        {
            mingleTracker.CheckProgression();
        }

        this.gameObject.SetActive(false);
    }

    string FormatText(string text, bool useItalics)
    {
        return useItalics ? $"<i>{text}</i>" : text;
    }
}