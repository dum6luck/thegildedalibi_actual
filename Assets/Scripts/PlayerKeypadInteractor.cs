using UnityEngine;

/* * SUMMARY:
 * This script runs globally on the player. It replaces all broad distance trigger 
 * volumes by checking if the camera is pointing directly at the lower cabinet door mesh.
 */

public class PlayerKeypadInteractor : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("How close do you need to stand to the door mesh to interact?")]
    public float interactRange = 2.5f;

    [Tooltip("Drag your main player camera game object here.")]
    public Transform playerCamera;

    void Start()
    {
        // Fallback check to auto-detect main rendering path if unassigned
        if (playerCamera == null)
        {
            playerCamera = Camera.main?.transform;
        }
    }

    void Update()
    {
        // Block processing if another UI layout has already paused time
        if (Time.timeScale == 0f || playerCamera == null) return;

        // Create a ray shooting out directly forward from center frame
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        RaycastHit hit;

        // Optional debugging tool: Draws a line in Scene View representing line-of-sight reach
        Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.yellow);

        if (Physics.Raycast(ray, out hit, interactRange))
        {
            // Verify that our reticle crosshair has targeted the door's specific tag identity
            if (hit.collider.CompareTag("KeypadDoor"))
            {
                // Trace up structural hierarchies to locate where the core script parameters sit
                KeypadSystem keypad = hit.collider.GetComponentInParent<KeypadSystem>();

                if (keypad != null && !keypad.keypadUI.activeSelf)
                {
                    // Require an active interaction validation key press
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        keypad.ToggleKeypad(true);
                    }
                }
            }
        }
    }
}