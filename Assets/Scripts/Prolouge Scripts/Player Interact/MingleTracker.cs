using UnityEngine;

public class MingleTracker : MonoBehaviour
{
    [Header("Progression Settings")]
    public int totalNPCsToTalkTo = 5;
    private int npcsTalkedToCount = 0;
    private bool hasTriggeredFinalThought = false;
    private bool finishedMingle = false; // NEW: Track if the whole event is done

    [Header("References")]
    public Talking_Manager talkingManager;

    public void CheckProgression()
    {
        npcsTalkedToCount++;
        Debug.Log("Mingle Progress: " + npcsTalkedToCount + "/" + totalNPCsToTalkTo);

        // If we hit the goal and haven't played the headache line yet
        if (npcsTalkedToCount >= totalNPCsToTalkTo && !hasTriggeredFinalThought)
        {
            hasTriggeredFinalThought = true;
            // Delay by 0.8 seconds to let the NPC dialogue UI fully fade out first
            Invoke("TriggerFinalThought", 0.8f);
        }
    }

    void TriggerFinalThought()
    {
        // Mark the mingle phase as officially finished
        finishedMingle = true;

        talkingManager.dialogueLines.Clear();
        Talking_Manager.DialogueLine thought = new Talking_Manager.DialogueLine();
        thought.characterName = "DETECTIVE";
        thought.sentence = "Ugh, I need the bathroom. These bright lights are making my head hurt.";
        thought.isItalic = true;

        talkingManager.dialogueLines.Add(thought);
        talkingManager.gameObject.SetActive(true);

        // 'false' ensures this monologue doesn't trigger the tracker again
        talkingManager.StartDialogueSequence(false);
    }

    // NEW: The door script will call this to see if the player is allowed to leave
    public bool HasFinishedMingle()
    {
        return finishedMingle;
    }
}