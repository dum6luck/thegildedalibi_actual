using UnityEngine;

[CreateAssetMenu(fileName = "Clue", menuName = "Case File/Clue")]
public class Clue : ScriptableObject
{
    public string clueName;

    [TextArea(3, 10)]
    public string clueDescription;

    public Sprite clueImage;
}