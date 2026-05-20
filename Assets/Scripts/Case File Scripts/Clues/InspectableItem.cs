using UnityEngine;
using UnityEngine.SceneManagement;

public class InspectableItem : MonoBehaviour
{
    [Header("Settings")]
    public string prefabResourcePath;
    public float interactDistance = 2.0f;

    [Header("Player Assignment (Crucial Fix)")]
    [Tooltip("Drag your root Player Object (the object with your movement script) from the Hierarchy into this slot.")]
    public GameObject mainPlayerObject;

    private Transform playerCamera;

    void Start()
    {
        if (Camera.main != null)
        {
            playerCamera = Camera.main.transform;

            // Safety backup: If you forget to assign it in the inspector, 
            // fallback to trying to find the highest parent object.
            if (mainPlayerObject == null)
            {
                mainPlayerObject = playerCamera.root.gameObject;
            }
        }
    }

    void Update()
    {
        if (playerCamera == null || mainPlayerObject == null) return;

        // Check distance between player camera and the item
        if (Vector3.Distance(transform.position, playerCamera.position) <= interactDistance)
        {
            // If the player presses 'I'
            if (Input.GetKeyDown(KeyCode.I))
            {
                // 1. Save the prefab path to our data carrier
                InspectionData.PrefabToLoad = prefabResourcePath;

                // 2. Clear out any physical forces acting on the player before we save coordinates
                Rigidbody rb = mainPlayerObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = Vector3.zero; // Changed from linearVelocity to velocity
                    rb.angularVelocity = Vector3.zero;
                }

                // 3. Save the MANUALLY assigned player's position and rotation
                InspectionData.LastPlayerPosition = mainPlayerObject.transform.position;
                InspectionData.LastPlayerRotation = mainPlayerObject.transform.rotation;
                InspectionData.ReturningFromInspection = true;

                // 4. Load your inspection scene
                SceneManager.LoadScene("InspectionScene");
            }
        }
    }
}