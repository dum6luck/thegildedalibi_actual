using UnityEngine;

public class ClueButton : MonoBehaviour
{
    public Clue clue;
    public CaseFileUI uiManager;

    public void OnClick()
    {
        uiManager.DisplayClue(clue);
    }
}