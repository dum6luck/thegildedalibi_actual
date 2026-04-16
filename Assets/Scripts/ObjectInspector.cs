using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Attach to the player camera.
///
/// Walking mode  — WASD moves, mouse looks. Press Q while aimed at an
///                 InspectableObject to enter inspection mode.
///
/// Inspection mode — WASD rotates the held object (W/S tilt, A/D spin).
///                   Mouse look and player movement are suspended.
///                   Press Escape to exit; the object returns to its pedestal.
/// </summary>
public class ObjectInspector : MonoBehaviour
{
    [Header("Pickup")]
    [Tooltip("Maximum distance at which Q can grab an object.")]
    public float PickupRange = 3f;

    [Tooltip("Distance in front of the camera the object is held at.")]
    public float HoldDistance = 1.5f;

    [Tooltip("How fast the object lerps to the hold position.")]
    public float HoldFollowSpeed = 10f;

    [Header("Rotation")]
    [Tooltip("Degrees per second the object rotates while WASD is held.")]
    public float RotationSpeed = 90f;

    private enum Mode { Walking, Inspecting }
    private Mode currentMode = Mode.Walking;

    private FPSController fpsController;
    private InspectableObject heldObject;
    private Rigidbody heldRigidbody;

    private readonly List<InspectableObject> returningObjects = new List<InspectableObject>();

    private void Awake()
    {
        fpsController = GetComponentInParent<FPSController>();
    }

    private void Update()
    {
        TickReturningObjects();

        switch (currentMode)
        {
            case Mode.Walking:    UpdateWalking();    break;
            case Mode.Inspecting: UpdateInspecting(); break;
        }
    }

    private void FixedUpdate()
    {
        if (currentMode != Mode.Inspecting || heldObject == null) return;

        // Keep the object smoothly in front of the camera while inspecting.
        Vector3 target = transform.position + transform.forward * HoldDistance;
        heldObject.transform.position = Vector3.Lerp(
            heldObject.transform.position, target, Time.fixedDeltaTime * HoldFollowSpeed);
    }

    // -------------------------------------------------------------------------
    // Walking mode
    // -------------------------------------------------------------------------

    /// <summary>Listens for Q to attempt a pickup.</summary>
    private void UpdateWalking()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TryPickup();
        }
    }

    /// <summary>Raycasts from the camera centre and enters inspection mode on hit.</summary>
    private void TryPickup()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, PickupRange)) return;

        InspectableObject target = hit.collider.GetComponentInParent<InspectableObject>();
        if (target == null) return;

        returningObjects.Remove(target);
        EnterInspection(target);
    }

    // -------------------------------------------------------------------------
    // Inspection mode
    // -------------------------------------------------------------------------

    private void EnterInspection(InspectableObject target)
    {
        heldObject = target;
        heldRigidbody = target.GetComponent<Rigidbody>();

        heldRigidbody.isKinematic = true;
        heldRigidbody.velocity = Vector3.zero;
        heldRigidbody.angularVelocity = Vector3.zero;

        // Suspend normal player input.
        fpsController.canMove = false;
        fpsController.canLook = false;

        currentMode = Mode.Inspecting;
    }

    /// <summary>
    /// WASD rotates the held object.
    /// A/D spin it left/right (world Y axis).
    /// W/S tilt it up/down (camera's right axis so it always feels relative to the view).
    /// Escape exits.
    /// </summary>
    private void UpdateInspecting()
    {
        float spin  = Input.GetAxis("Horizontal") * RotationSpeed * Time.deltaTime;
        float tilt  = Input.GetAxis("Vertical")   * RotationSpeed * Time.deltaTime;

        heldObject.transform.Rotate(Vector3.up,        -spin, Space.World);
        heldObject.transform.Rotate(transform.right,    tilt, Space.World);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitInspection();
        }
    }

    private void ExitInspection()
    {
        fpsController.canMove = true;
        fpsController.canLook = true;

        heldRigidbody.isKinematic = false;

        if (heldObject.ReturnOnDrop)
        {
            returningObjects.Add(heldObject);
        }
        else
        {
            heldRigidbody.useGravity = true;
        }

        heldObject = null;
        heldRigidbody = null;

        currentMode = Mode.Walking;
    }

    // -------------------------------------------------------------------------
    // Object return
    // -------------------------------------------------------------------------

    /// <summary>Lerps released objects back to their original transform each frame.</summary>
    private void TickReturningObjects()
    {
        for (int i = returningObjects.Count - 1; i >= 0; i--)
        {
            InspectableObject obj = returningObjects[i];

            if (obj == null) { returningObjects.RemoveAt(i); continue; }

            float step = obj.ReturnSpeed * Time.deltaTime;
            obj.transform.position = Vector3.Lerp(obj.transform.position, obj.OriginalPosition, step);
            obj.transform.rotation = Quaternion.Slerp(obj.transform.rotation, obj.OriginalRotation, step);

            bool arrived =
                Vector3.Distance(obj.transform.position, obj.OriginalPosition) < 0.01f &&
                Quaternion.Angle(obj.transform.rotation, obj.OriginalRotation) < 0.5f;

            if (arrived)
            {
                obj.transform.SetPositionAndRotation(obj.OriginalPosition, obj.OriginalRotation);
                returningObjects.RemoveAt(i);
            }
        }
    }
}
