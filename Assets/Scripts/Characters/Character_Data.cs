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

    private Dictionary<string, Sprite> overworld_dict = new Dictionary<string, Sprite>();
    private Dictionary<string, Sprite> dialogue_dict = new Dictionary<string, Sprite>();

    private void OnEnable() {
        int overworld_dict_len = overworld_emotions.Count;
        int dialogue_dict_len = dialogue_emotions.Count;

        if (overworld_emotions.Count > overworld_sprites.Count) overworld_dict_len = overworld_sprites.Count;
        if (dialogue_emotions.Count > dialogue_sprites.Count) dialogue_dict_len = dialogue_sprites.Count;

        for (int i = 0; i < overworld_dict_len; i++)
        {
            overworld_dict[overworld_emotions[i]] = overworld_sprites[i];
        }

        for (int i = 0; i < dialogue_dict_len; i++)
        {
            dialogue_dict[dialogue_emotions[i]] = dialogue_sprites[i];
        }
    }

    public Sprite Get_Overworld_Sprite(string emotion) {
        return !string.IsNullOrEmpty(emotion) && overworld_dict.ContainsKey(emotion) ? overworld_dict[emotion] : overworld_dict["neutral"];
    }

    public Sprite Get_Dialogue_Sprite(string emotion) {
        return !string.IsNullOrEmpty(emotion) && dialogue_dict.ContainsKey(emotion) ? dialogue_dict[emotion] : dialogue_dict["neutral"];
    }
}