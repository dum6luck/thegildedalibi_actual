using UnityEngine;
using System.Collections.Generic;

public class NPCData : MonoBehaviour
{
    [System.Serializable]
    public struct ConversationLine
    {
        public Character_Data speaker;
        public string emotion;
        [TextArea(3, 10)] public string sentence;
        public bool isItalic;
    }

    [Header("NPC Identity")]
    public Character_Data npc; // <-- SET THIS TO "JULIAN", "IRIS", ETC. IN THE INSPECTOR
    public SpriteRenderer sprite;

    [Header("First Time Conversation")]
    public List<ConversationLine> conversation;

    [Header("Repeat Conversation")]
    public ConversationLine repeatLine;

    [HideInInspector] public bool hasTalked = false;

    public void Interact(Talking_Manager manager)
    {
        manager.dialogueLines.frames.Clear();

        // Pass the NPC's actual name to the manager before starting
        manager.SetCurrentNPC(npc, sprite);

        if (!hasTalked)
        {
            foreach (var line in conversation)
            {
                manager.dialogueLines.frames.Add(new OverworldCutsceneFrame
                {
                    speaker = line.speaker == null ? npc : line.speaker,
                    dialogueLine = line.sentence,
                    isItalic = line.isItalic,
                    emotion = line.emotion
                });
            }
            hasTalked = true;
            manager.StartDialogueSequence(true);
        }
        else
        {
            manager.dialogueLines.frames.Add(new OverworldCutsceneFrame
            {
                speaker = repeatLine.speaker == null ? npc : repeatLine.speaker,
                dialogueLine = repeatLine.sentence,
                isItalic = repeatLine.isItalic,
                emotion = repeatLine.emotion
            });
            manager.StartDialogueSequence(false);
        }
    }
}