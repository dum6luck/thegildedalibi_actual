using UnityEngine;
using UnityEngine.SceneManagement;

public class InspectionController : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotateSpeed = 150f; // Speed of the keyboard rotation

    private GameObject spawnedItem;

    void Start()
    {
        if (!string.IsNullOrEmpty(InspectionData.PrefabToLoad))
        {
            GameObject prefab = Resources.Load<GameObject>(InspectionData.PrefabToLoad);
            if (prefab != null)
            {
                // Spawn perfectly at center
                spawnedItem = Instantiate(prefab, Vector3.zero, Quaternion.identity);

                // Safety cleanup of physics so nothing drifts
                Rigidbody itemRb = spawnedItem.GetComponent<Rigidbody>();
                if (itemRb != null) Destroy(itemRb);

                Collider itemCol = spawnedItem.GetComponent<Collider>();
                if (itemCol != null) itemCol.enabled = false;
            }
        }
    }

    void Update()
    {
        if (spawnedItem != null)
        {
            // Get input from WASD or Arrow keys
            float inputX = Input.GetAxis("Horizontal"); // A/D or Left/Right
            float inputY = Input.GetAxis("Vertical");   // W/S or Up/Down

            // If any key is being pressed, rotate the item
            if (Mathf.Abs(inputX) > 0.01f || Mathf.Abs(inputY) > 0.01f)
            {
                // A/D rotates around the vertical axis (Y-axis)
                spawnedItem.transform.Rotate(Vector3.up, -inputX * rotateSpeed * Time.deltaTime, Space.World);

                // W/S rotates around the horizontal axis (X-axis)
                spawnedItem.transform.Rotate(Vector3.right, inputY * rotateSpeed * Time.deltaTime, Space.World);
            }
        }

        // Press Escape or 'I' to safely go back
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.I))
        {
            // CRITICAL: Double check this string matches your Main Museum scene name exactly!
            SceneManager.LoadScene("Main_Game");
        }
    }
}