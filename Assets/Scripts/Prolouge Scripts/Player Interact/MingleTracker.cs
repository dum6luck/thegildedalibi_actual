using UnityEngine;
using System.Collections.Generic;

public class MingleTracker : MonoBehaviour
{
    public int totalNPCsToTalkTo = 5;
    private HashSet<string> npcsTalkedTo = new HashSet<string>();
    private bool finished = false;

    public Talking_Manager talkingManager;

    public void CheckProgression(string name)
    {
        if (npcsTalkedTo.Add(name)) // Only true if name is new
        {
            Debug.Log($"Talked to {name}. Progress: {npcsTalkedTo.Count}/{totalNPCsToTalkTo}");
        }

        if (npcsTalkedTo.Count >= totalNPCsToTalkTo && !finished)
        {
            finished = true;
            Invoke(nameof(TriggerFinalThought), 1.0f);
        }
    }

    void TriggerFinalThought()
    {
        talkingManager.dialogueLines.Clear();
        talkingManager.dialogueLines.Add(new Talking_Manager.DialogueLine
        {
            characterName = "DETECTIVE",
            sentence = "Ugh, I need the bathroom. These bright lights are making my head hurt.",
            isItalic = true
        });
        talkingManager.gameObject.SetActive(true);
        talkingManager.StartDialogueSequence(false); // false so it doesn't loop
    }
}