using System.Collections.Generic;
using UnityEngine;

public class ClueManager : MonoBehaviour
{
    public static ClueManager Instance;

    // This list holds every clue the player has clicked on
    public List<Clue> collectedClues = new List<Clue>();

    void Awake()
    {
        // Singleton pattern: keeps this object alive between scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddClue(Clue newClue)
    {
        if (!collectedClues.Contains(newClue))
        {
            collectedClues.Add(newClue);
            Debug.Log("Clue added: " + newClue.clueName);
        }
    }
}