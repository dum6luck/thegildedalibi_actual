using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueSystem : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public GameObject panel;

    [Header("Typing")]
    public float typingSpeed = 0.03f;

    private Queue<DialogueLine> lines = new Queue<DialogueLine>();
    private Coroutine typingRoutine;
    private bool isTyping;

    public static DialogueSystem Instance;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (!panel.activeSelf) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                SkipTyping();
            }
            else
            {
                DisplayNext();
            }
        }
    }

    public void StartDialogue(List<DialogueLine> dialogue)
    {
        panel.SetActive(true);

        lines.Clear();

        foreach (var line in dialogue)
        {
            lines.Enqueue(line);
        }

        DisplayNext();
    }

    void DisplayNext()
    {
        if (lines.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = lines.Dequeue();

        nameText.text = line.characterName;

        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        typingRoutine = StartCoroutine(TypeLine(line.sentence));
    }

    IEnumerator TypeLine(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in sentence)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    void SkipTyping()
    {
        StopCoroutine(typingRoutine);
        isTyping = false;
    }

    void EndDialogue()
    {
        panel.SetActive(false);
    }
}

[System.Serializable]
public class DialogueLine
{
    public string characterName;
    [TextArea] public string sentence;
}