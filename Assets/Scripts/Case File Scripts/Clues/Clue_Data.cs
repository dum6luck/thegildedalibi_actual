using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class NPC_Reaction
{
    public string npc_name;
    [TextArea(3, 5)]
    public string reaction_text;
}

[CreateAssetMenu(fileName = "New_Clue", menuName = "DetectiveSystem/Clue_Data")]
public class Clue_Data : ScriptableObject
{
    public string clue_title;
    [TextArea(3, 10)]
    public string clue_description;
    public Sprite clue_icon;
    [TextArea(3, 5)]
    public string collection_dialogue;

    public Cutscene_Data clue_cutscene;

    // NEW: List of reactions for different characters
    public List<NPC_Reaction> npc_reactions;
}