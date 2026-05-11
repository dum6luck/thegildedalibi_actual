using System;
using UnityEngine;

/// <summary>
/// Attach to a child GameObject of an InspectableObject to mark a clickable clue.
/// InspectionScreen raycasts against this object's collider during inspection;
/// when the player clicks it, Reveal() fires OnClueDiscovered so the screen
/// can display the clue text.
/// </summary>
public class ArtifactClue : MonoBehaviour
{
    [Tooltip("Text shown when the player clicks this clue during inspection.")]
    [TextArea(2, 6)]
    public string ClueText = "A hidden inscription...";

    /// <summary>Raised whenever any clue is clicked. InspectionScreen subscribes to this.</summary>
    public static event Action<string> OnClueDiscovered;

    /// <summary>Called by InspectionScreen when the player left-clicks this collider.</summary>
    public void Reveal()
    {
        OnClueDiscovered?.Invoke(ClueText);
    }
}
