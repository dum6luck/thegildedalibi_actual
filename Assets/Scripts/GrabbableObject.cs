using UnityEngine;

/// <summary>
/// Marks an object as inspectable in the museum.
/// Add this component to any prop the player should be able to pick up and examine.
/// Requires a Rigidbody so physics can be toggled during inspection.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class GrabbableObject : MonoBehaviour
{
    [Tooltip("Display name shown when the player hovers over this object.")]
    public string DisplayName = "Object";

    [Tooltip("If true, the object snaps back to its original position and rotation when dropped.")]
    public bool ReturnOnDrop = true;

    [Tooltip("How fast the object lerps back to its original transform when returned.")]
    public float ReturnSpeed = 5f;

    // Stored on Start so we always know where to return the object.
    [HideInInspector] public Vector3 OriginalPosition;
    [HideInInspector] public Quaternion OriginalRotation;

    private void Start()
    {
        OriginalPosition = transform.position;
        OriginalRotation = transform.rotation;
    }
}
