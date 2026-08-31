using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct DialogueFrame
{
    [Header("Character Sprites")]
    public string emotion;

    [Header("Text Formatting")]
    public bool isItalic;

    [TextArea(3, 5)]
    public string dialogueLine;
}

[CreateAssetMenu(fileName = "New_Dialogue", menuName = "Dialogue System/New Overworld Dialogue")]
public class OverworldDialogueData : ScriptableObject
{
    public List<DialogueFrame> frames = new List<DialogueFrame>();
}