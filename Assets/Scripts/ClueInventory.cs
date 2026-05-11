using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton that collects and stores all discovered clue texts.
/// Subscribes to both ArtifactClue.OnClueDiscovered and
/// InspectableClue.OnAnyClueDiscovered automatically.
/// </summary>
public class ClueInventory : MonoBehaviour
{
    public static ClueInventory Instance { get; private set; }

    /// <summary>All clue strings collected so far, in discovery order.</summary>
    public IReadOnlyList<string> Clues => clues;

    private readonly List<string> clues = new List<string>();

    public delegate void ClueAddedHandler(string clueText);

    /// <summary>Fired whenever a new clue is added to the inventory.</summary>
    public static event ClueAddedHandler OnClueAdded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        ArtifactClue.OnClueDiscovered       += AddClue;
        InspectableClue.OnAnyClueDiscovered += AddClue;
    }

    private void OnDisable()
    {
        ArtifactClue.OnClueDiscovered       -= AddClue;
        InspectableClue.OnAnyClueDiscovered -= AddClue;
    }

    /// <summary>Adds a clue if it is not already in the inventory, then fires OnClueAdded.</summary>
    public void AddClue(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        if (clues.Contains(text)) return;

        clues.Add(text);
        OnClueAdded?.Invoke(text);
    }
}
