using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class Talking_Manager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI nameDisplay;
    public TextMeshProUGUI dialogueDisplay;
    public GameObject nextArrow;
    public GameObject dialoguePanel; // The main UI box containing the text

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

    void Start()
    {
        // Setup initial cursor state (MingleSceneManager will override this)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (nextArrow != null) nextArrow.SetActive(false);

        if (nameDisplay != null) nameDisplay.text = "";
        if (dialogueDisplay != null) dialogueDisplay.text = "";
    }

    public void StartDialogueSequence()
    {
        // Force mouse to be free when dialogue starts
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        index = 0;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        DisplayLine();
    }

    public void AdvanceDialogue()
    {
        if (isTyping)
        {
            // SKIP: Finish the text instantly
            StopCoroutine(typingCoroutine);
            isTyping = false;
            dialogueDisplay.text = FormatText(dialogueLines[index].sentence, dialogueLines[index].isItalic);

            if (nextArrow != null) nextArrow.SetActive(true);
            return;
        }

        // NEXT LINE: Advance if there are more sentences
        if (index < dialogueLines.Count - 1)
        {
            index++;
            if (nextArrow != null) nextArrow.SetActive(false);
            DisplayLine();
        }
        else
        {
            // --- THE UPDATED ELSE BLOCK ---
            Debug.Log("End of dialogue. Closing panel and signaling Scene Manager.");

            if (nextArrow != null) nextArrow.SetActive(false);

            // Hide the UI so the player can see the world
            if (dialoguePanel != null) dialoguePanel.SetActive(false);

            // Disable this object. MingleSceneManager sees this and unlocks the player.
            this.gameObject.SetActive(false);
        }
    }

    void DisplayLine()
    {
        if (dialogueLines.Count == 0) return;

        nameDisplay.text = dialogueLines[index].characterName;
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
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

    string FormatText(string text, bool useItalics)
    {
        return useItalics ? $"<i>{text}</i>" : text;
    }
}