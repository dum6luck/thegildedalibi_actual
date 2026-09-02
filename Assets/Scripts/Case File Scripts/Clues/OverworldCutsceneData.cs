using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct OverworldCutsceneFrame
{
    [Header("Character Sprites")]
    public string emotion;

    [Header("Focus & Dialogue")]
    public Character_Data speaker;

    [Header("Text Formatting")]
    public bool isItalic;

    [TextArea(3, 5)]
    public string dialogueLine;
}

[CreateAssetMenu(fileName = "New_Cutscene", menuName = "Dialogue System/New Overworld Cutscene")]
public class OverworldCutsceneData : ScriptableObject
{
    public List<OverworldCutsceneFrame> frames = new List<OverworldCutsceneFrame>();
}