using UnityEngine;

public class LockedDisplayCase : MonoBehaviour
{
    [Header("Character Identity")]
    public string characterName = "DETECTIVE";

    [Header("Clue Data Customization")]
    public Clue_Data daggerClueInfo;

    [Header("Dialogue Lines")]
    [TextArea(3, 5)]
    public string lockedText = "It's locked tight. Looks like it requires a specific key.";

    [TextArea(3, 5)]
    public string unlockText = "With that key I unlocked it, now I can inspect the knife.";

    [TextArea(3, 5)]
    public string postInspectionThoughts = "The blade depth doesn't match the victim's stab wounds, the edge is completely blunted, and with no deep blood residue in the crevices or wear on the handle, this couldn't have been the murder weapon.";

    [Header("Settings")]
    public float interactDistance = 2.5f;

    [Header("References")]
    public GameObject playerObject;
    public InspectableItem inspectableItemScript;

    private PlayerInventory playerInventory;
    private Transform playerCamera;
    private bool isCaseUnlocked = false;
    private float interactCooldown = 0.5f;
    private float nextInteractTime = 0f;

    void Start()
    {
        if (Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }

        if (playerObject != null)
        {
            playerInventory = playerObject.GetComponent<PlayerInventory>();
        }

        // FIX: Verify that the script component is valid and get its path settings
        bool cameFromDaggerInspection = false;
        if (inspectableItemScript != null)
        {
            // If the last item we inspected matches the exact path of THIS specific item script
            if (InspectionData.LastInspectedPrefabName == inspectableItemScript.prefabResourcePath && !string.IsNullOrEmpty(InspectionData.LastInspectedPrefabName))
            {
                cameFromDaggerInspection = true;
            }
        }

        // --- EXCLUSIVE POST-INSPECTION RUN ---
        if (cameFromDaggerInspection)
        {
            // Clear out the tracking slot so it doesn't trigger repeatedly
            InspectionData.LastInspectedPrefabName = "";

            isCaseUnlocked = true;
            if (inspectableItemScript != null) inspectableItemScript.isLocked = false;

            Invoke("HandlePostInspectionLogistics", 0.2f);
        }
        else
        {
            // Set default locked status on a normal room load
            if (inspectableItemScript != null && !isCaseUnlocked)
            {
                inspectableItemScript.isLocked = true;
                inspectableItemScript.requiresKey = true;
            }
        }
    }

    void HandlePostInspectionLogistics()
    {
        if (Dialogue_Manager.Instance != null)
        {
            Dialogue_Manager.Instance.Show_Dialogue(characterName, postInspectionThoughts);
        }

        Case_File_UI uiManager = FindObjectOfType<Case_File_UI>();
        if (uiManager != null && daggerClueInfo != null)
        {
            uiManager.Add_Clue_To_Log(daggerClueInfo);
            Debug.Log("[Journal System] Dagger evidence cleanly saved.");
        }
    }

    void Update()
    {
        if (playerCamera == null || playerInventory == null) return;

        if (Vector3.Distance(transform.position, playerCamera.position) <= interactDistance)
        {
            if (Input.GetKeyDown(KeyCode.E) && Time.time >= nextInteractTime)
            {
                nextInteractTime = Time.time + interactCooldown;
                InteractWithCase();
            }
        }
    }

    public void InteractWithCase()
    {
        if (Dialogue_Manager.Instance == null) return;
        if (isCaseUnlocked) return;

        if (playerInventory.hasDisplayCaseKey)
        {
            isCaseUnlocked = true;
            if (inspectableItemScript != null) inspectableItemScript.isLocked = false;

            Dialogue_Manager.Instance.Show_Dialogue(characterName, unlockText);
        }
        else
        {
            Dialogue_Manager.Instance.Show_Dialogue(characterName, lockedText);
        }
    }
}