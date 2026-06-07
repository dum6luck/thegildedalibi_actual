using UnityEngine;
using UnityEngine.SceneManagement;

public class InspectableItem : MonoBehaviour
{
    [Header("Settings")]
    public string prefabResourcePath;
    public float interactDistance = 2.0f;

    [Header("Lock System settings")]
    public bool isLocked = false;
    public bool requiresKey = false;

    [Header("Player Assignment (Crucial Fix)")]
    public GameObject mainPlayerObject;

    private Transform playerCamera;

    void Start()
    {
        if (Camera.main != null)
        {
            playerCamera = Camera.main.transform;

            if (mainPlayerObject == null)
            {
                mainPlayerObject = playerCamera.root.gameObject;
            }
        }
    }

    void Update()
    {
        if (playerCamera == null || mainPlayerObject == null) return;

        if (Vector3.Distance(transform.position, playerCamera.position) <= interactDistance)
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                if (isLocked)
                {
                    Debug.LogWarning("Cannot inspect: This item is locked inside the display case!");
                    return;
                }

                LoadInspectionRoom();
            }
        }
    }

    void LoadInspectionRoom()
    {
        InspectionData.PrefabToLoad = prefabResourcePath;

        // FIX: Store the exact prefab filename into the cross-scene memory slot
        InspectionData.LastInspectedPrefabName = prefabResourcePath;

        // Clear player velocity
        Rigidbody rb = mainPlayerObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        InspectionData.LastPlayerPosition = mainPlayerObject.transform.position;
        InspectionData.LastPlayerRotation = mainPlayerObject.transform.rotation;
        InspectionData.ReturningFromInspection = true;

        SceneManager.LoadScene("InspectionScene");
    }
}