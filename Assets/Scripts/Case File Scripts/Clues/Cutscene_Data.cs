using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct CutsceneFrame
{
    [Header("Character Sprites")]
    public Sprite leftCharacterSprite;
    public Sprite rightCharacterSprite;

    public enum ActiveSpeaker { Left, Right, Neither }
    [Header("Focus & Dialogue")]
    public ActiveSpeaker activeSpeaker;
    public Character_Data speaker;

    [Header("Text Formatting")]
    public bool isItalic;

    [TextArea(3, 5)]
    public string dialogueLine;
}

[CreateAssetMenu(fileName = "New_Cutscene", menuName = "Dialogue System/New Still-Image Cutscene")]
public class Cutscene_Data : ScriptableObject
{
    [Header("Cutscene Background")]
    public Sprite backgroundSprite;

    public List<CutsceneFrame> frames = new List<CutsceneFrame>();
}