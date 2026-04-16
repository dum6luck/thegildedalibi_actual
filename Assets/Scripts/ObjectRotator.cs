using UnityEngine;

/// <summary>
/// Rotates a target Transform with WASD input during item inspection.
/// A / D  — spin the object left and right (world Y axis).
/// W / S  — tilt the object toward and away from the camera (world X axis).
/// Activated and deactivated by InspectionScreen.
/// </summary>
public class ObjectRotator : MonoBehaviour
{
    [Tooltip("Degrees per second applied by A / D.")]
    public float SpinSpeed = 120f;

    [Tooltip("Degrees per second applied by W / S.")]
    public float TiltSpeed = 90f;

    private Transform rotationTarget;

    private void Awake()
    {
        // Start disabled; InspectionScreen enables this when inspection begins.
        enabled = false;
    }

    /// <summary>Assigns the target and begins reading input.</summary>
    public void StartRotating(Transform target)
    {
        rotationTarget = target;
        enabled = true;
    }

    /// <summary>Clears the target and stops reading input.</summary>
    public void StopRotating()
    {
        enabled = false;
        rotationTarget = null;
    }

    private void Update()
    {
        if (rotationTarget == null) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        rotationTarget.Rotate(Vector3.up,   -h * SpinSpeed * Time.deltaTime, Space.World);
        rotationTarget.Rotate(Vector3.right,  v * TiltSpeed  * Time.deltaTime, Space.World);
    }
}
