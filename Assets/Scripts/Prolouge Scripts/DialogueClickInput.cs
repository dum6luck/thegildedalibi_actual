using UnityEngine;

public class DialogueClickInput : MonoBehaviour
{
    public Talking_Manager talkingManager;

    void Update()
    {
        // If the dialogue panel is active, listen for left-clicks
        if (talkingManager != null && talkingManager.gameObject.activeSelf)
        {
            if (Input.GetMouseButtonDown(0))
            {
                talkingManager.AdvanceDialogue();
            }
        }
    }
}