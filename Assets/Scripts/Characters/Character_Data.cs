using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New_Character", menuName = "Dialogue System/New Character")]
public class Character_Data : ScriptableObject
{
    public string name;
    public string initial_emotion = "neutral";

    public List<AudioClip> voiceSamples;

    [Header("Dictionary of Overworld Emotions For Each Image")]
    public List<string> overworld_emotions = new List<string>();
    public List<Sprite> overworld_sprites = new List<Sprite>();

    [Header("Dictionary of Dialogue Emotions For Each Image")]
    public List<string> dialogue_emotions = new List<string>();
    public List<Sprite> dialogue_sprites = new List<Sprite>();
}