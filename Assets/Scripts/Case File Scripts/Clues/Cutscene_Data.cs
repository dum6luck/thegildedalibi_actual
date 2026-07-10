using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New_Cutscene", menuName = "Dialogue System/New Still-Image Cutscene")]
public class Cutscene_Data : ScriptableObject
{
    public List<Sprite> images;

    [Header("Dictionary of Dialogues For Each Image")]
    public List<string> names = new List<string>();
    public List<string> dialogue_lines = new List<string>();
}