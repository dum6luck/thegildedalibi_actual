using System;
using UnityEngine;

/// <summary>
/// Place on a child GameObject of an InspectableObject to mark a hidden clue.
/// When the player clicks it while inspecting the parent object, OnAnyClueDiscovered fires.
/// </summary>
public class InspectableClue : MonoBehaviour
{
    [Tooltip("The clue text revealed when the player clicks this hotspot.")]
    [TextArea(2, 5)]
    public string ClueText = "A hidden inscription...";

    /// <summary>
    /// Global event raised whenever any clue is discovered.
    /// ClueUI subscribes to this to display the text.
    /// </summary>
    public static event Action<string> OnAnyClueDiscovered;

    /// <summary>Called by ObjectInspector when the player clicks this clue while holding the parent object.</summary>
    public void Discover()
    {
        OnAnyClueDiscovered?.Invoke(ClueText);
    }
}
