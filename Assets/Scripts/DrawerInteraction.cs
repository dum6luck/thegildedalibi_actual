using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class DrawerInteraction : MonoBehaviour
{
    [Header("Item Requirement")]
    [SerializeField] private string requiredItemID = "Gem";
    [SerializeField] private bool consumeItemOnUse = false;

    [Header("Reward Item")]
    [SerializeField] private string itemToRewardID = "USB"; // Added item ID reward

    [Header("Slide Settings")]
    [Tooltip("Local distance to slide (e.g., Z = 0.5 moves forward half a meter).")]
    [SerializeField] private Vector3 openOffset = new Vector3(0f, 0f, 0.5f);
    [SerializeField] private float slideDuration = 1.0f;

    [Header("Dialogue Content")]
    [TextArea(2, 4)]
    [SerializeField] private string lockedMessage = "The drawer is locked tightly. It looks like it needs a gem to open.";

    [TextArea(2, 4)]
    [SerializeField] private string unlockedMessage = "You placed the gem into the drawer slot. It unlocked!";

    [Header("Clue Reference")]
    [SerializeField] private Interactable_Clue attachedClue;

    [Header("Interaction Prompt")]
    [SerializeField] private LensSystem cameraLensSystem;

    private Dialogue_Manager dialogueManager;
    private bool isOpen = false;
    private bool isMoving = false;
    private bool isInteracting = false;
    private bool is_player_nearby = false;

    private void Start()
    {
        dialogueManager = FindObjectOfType<Dialogue_Manager>();

        if (cameraLensSystem == null && Camera.main != null)
        {
            cameraLensSystem = Camera.main.GetComponent<LensSystem>();
        }

        if (attachedClue != null)
        {
            attachedClue.enabled = false;
        }
    }

    private void Update()
    {
        if (is_player_nearby && !isOpen && !isMoving && !isInteracting && Input.GetKeyDown(KeyCode.E))
        {
            InteractWithDrawer();
        }
    }

    public void InteractWithDrawer()
    {
        if (isOpen || isMoving || isInteracting) return;

        bool hasGem = (UserInventory.Instance != null && UserInventory.Instance.HasItem(requiredItemID));

        if (hasGem)
        {
            StartCoroutine(HandleUnlockedDrawerSequence());
        }
        else
        {
            StartCoroutine(HandleLockedDrawerSequence());
        }
    }

    private IEnumerator HandleLockedDrawerSequence()
    {
        isInteracting = true;

        if (dialogueManager != null)
        {
            dialogueManager.Show_Dialogue("DETECTIVE", lockedMessage);
        }

        yield return null;
        yield return null;

        while (dialogueManager != null && dialogueManager.Is_Dialogue_Ongoing())
        {
            if (Input.GetMouseButtonDown(0))
            {
                dialogueManager.SendMessage("SkipTypewriter", SendMessageOptions.DontRequireReceiver);
                dialogueManager.SendMessage("Finish_Typing", SendMessageOptions.DontRequireReceiver);
            }
            yield return null;
        }

        isInteracting = false;
    }

    private IEnumerator HandleUnlockedDrawerSequence()
    {
        isInteracting = true;

        if (dialogueManager != null)
        {
            dialogueManager.Show_Dialogue("DETECTIVE", unlockedMessage);
        }

        yield return null;
        yield return null;

        bool dialogueFinished = false;
        while (!dialogueFinished)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (dialogueManager != null && dialogueManager.Is_Dialogue_Ongoing())
                {
                    dialogueManager.SendMessage("SkipTypewriter", SendMessageOptions.DontRequireReceiver);
                    dialogueManager.SendMessage("Finish_Typing", SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    dialogueFinished = true;
                }
            }
            yield return null;
        }

        isInteracting = false;

        yield return StartCoroutine(OpenDrawerCoroutine());

        if (consumeItemOnUse && UserInventory.Instance != null)
        {
            UserInventory.Instance.RemoveItem(requiredItemID);
        }

        // Add USB to inventory
        if (!string.IsNullOrEmpty(itemToRewardID) && UserInventory.Instance != null)
        {
            UserInventory.Instance.AddItem(itemToRewardID);
        }

        if (attachedClue != null)
        {
            attachedClue.enabled = true;
            attachedClue.Collect();
        }
    }

    private IEnumerator OpenDrawerCoroutine()
    {
        isMoving = true;

        Vector3 startLocalPos = transform.localPosition;
        Vector3 targetLocalPos = startLocalPos + openOffset;
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / slideDuration);
            transform.localPosition = Vector3.Lerp(startLocalPos, targetLocalPos, t);
            yield return null;
        }

        transform.localPosition = targetLocalPos;
        isOpen = true;
        isMoving = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            is_player_nearby = true;
            if (cameraLensSystem != null) cameraLensSystem.ShowCluePrompt();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            is_player_nearby = false;
            if (cameraLensSystem != null) cameraLensSystem.HideCluePrompt();
        }
    }
}