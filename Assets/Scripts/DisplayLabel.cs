using UnityEngine;

// Ensure filename is DisplayLabel.cs
public class DisplayLabel : MonoBehaviour
{
    [Header("Detective's Thoughts")]
    public string characterName = "DETECTIVE";

    [TextArea(3, 10)]
    public string description = "This vase looks expensive... and fragile.";

    public void ShowDescription()
    {
        // Make sure this matches the name of your script (Dialogue_Manager)
        // and the function name (Show_Dialogue)
        if (Dialogue_Manager.Instance != null)
        {
            Dialogue_Manager.Instance.Show_Dialogue(characterName, description);
        }
    }
}