using UnityEngine;
using System.Collections.Generic;

public class NPCData : MonoBehaviour
{
    [System.Serializable]
    public struct ConversationLine
    {
        public string characterName;
        [TextArea(3, 10)]
        public string sentence;
        public bool isItalic;
    }

    [Header("First Time Conversation")]
    public List<ConversationLine> conversation;

    [Header("Repeat Conversation")]
    [Tooltip("The line they say after the first meeting.")]
    public ConversationLine repeatLine;

    [HideInInspector] public bool hasTalked = false; // Memory switch
}