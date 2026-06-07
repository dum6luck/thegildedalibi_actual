using UnityEngine;

public class LensGem : MonoBehaviour
{
    public enum GemType { Blue, Red }
    [Header("Gem Properties")]
    public GemType gemType; // Choose Blue or Red in the inspector dropdown

    private LensSystem lensSystem;
    private bool playerInZone = false;

    void Start()
    {
        // Automatically finds the LensSystem component attached to your Main Camera
        if (Camera.main != null)
        {
            lensSystem = Camera.main.GetComponent<LensSystem>();
        }
    }

    void Update()
    {
        // Checks every frame if the player is within the trigger radius AND hits E
        if (playerInZone && Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    public void Interact()
    {
        if (lensSystem != null)
        {
            // Sends the gemType name string ("Blue" or "Red") over to the camera script
            lensSystem.ChargeLens(gemType.ToString());
            Debug.Log($"{gemType} Gem successfully activated with 'E' key!");
        }
    }

    // Trigger zone detection
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
            Debug.Log($"Player entered range. Press 'E' to use the {gemType} Gem.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
        }
    }
}