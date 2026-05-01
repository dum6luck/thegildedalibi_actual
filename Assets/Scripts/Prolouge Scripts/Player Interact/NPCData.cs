using UnityEngine;
using System.Collections.Generic;

public class NPCData : MonoBehaviour
{
    [System.Serializable]
    public struct ConversationLine
    {
        public string characterName;
        [TextArea(3, 10)] public string sentence;
        public bool isItalic;
    }

    [Header("NPC Identity")]
    public string npcName; // <-- SET THIS TO "JULIAN", "IRIS", ETC. IN THE INSPECTOR

    [Header("First Time Conversation")]
    public List<ConversationLine> conversation;

    [Header("Repeat Conversation")]
    public ConversationLine repeatLine;

    [HideInInspector] public bool hasTalked = false;

    public void Interact(Talking_Manager manager)
    {
        manager.dialogueLines.Clear();

        // Pass the NPC's actual name to the manager before starting
        manager.SetCurrentNPC(npcName);

        if (!hasTalked)
        {
            foreach (var line in conversation)
            {
                manager.dialogueLines.Add(new Talking_Manager.DialogueLine
                {
                    characterName = line.characterName,
                    sentence = line.sentence,
                    isItalic = line.isItalic
                });
            }
            hasTalked = true;
            manager.StartDialogueSequence(true);
        }
        else
        {
            manager.dialogueLines.Add(new Talking_Manager.DialogueLine
            {
                characterName = repeatLine.characterName,
                sentence = repeatLine.sentence,
                isItalic = repeatLine.isItalic
            });
            manager.StartDialogueSequence(false);
        }
    }
}