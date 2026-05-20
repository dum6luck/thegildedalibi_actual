using UnityEngine;
using System.Collections;

public class PlayerPositionRestorer : MonoBehaviour
{
    void Start()
    {
        // Check if we are actually returning from looking at an item
        if (InspectionData.ReturningFromInspection)
        {
            StartCoroutine(TeleportPlayerRoutine());
        }
    }

    private IEnumerator TeleportPlayerRoutine()
    {
        // Wait for the very end of the frame so all other scripts 
        // (like spawn managers and movement scripts) finish initializing first
        yield return new WaitForEndOfFrame();

        // 1. Temporarily disable physics controllers so they don't fight the teleport
        CharacterController cc = GetComponent<CharacterController>();
        Rigidbody rb = GetComponent<Rigidbody>();

        if (cc != null) cc.enabled = false;
        if (rb != null) rb.isKinematic = true; // Prevents gravity from pulling you down mid-teleport

        // 2. Perform the actual teleport to the saved coordinates
        transform.position = InspectionData.LastPlayerPosition;
        transform.rotation = InspectionData.LastPlayerRotation;

        // 3. Force child objects (like your camera head) to align correctly too
        foreach (Transform child in transform)
        {
            // If your camera rotates independently, this ensures it looks the right way
            if (child.CompareTag("MainCamera"))
            {
                child.rotation = InspectionData.LastPlayerRotation;
            }
        }

        // Wait one more split second for the position change to register in Unity's engine
        yield return null;

        // 4. Turn everything back on so you can move normally
        if (cc != null) cc.enabled = true;
        if (rb != null) rb.isKinematic = false;

        // Clear the flag so normal restarts work down the line
        InspectionData.ReturningFromInspection = false;

        Debug.Log("Player successfully restored to position: " + transform.position);
    }
}