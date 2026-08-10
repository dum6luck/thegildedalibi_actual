using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New_Character", menuName = "Dialogue System/New Character")]
public class Character_Data : ScriptableObject
{
    public string name;
    public string initial_emotion = "neutral";

    public List<AudioClip> voiceSamples;
}