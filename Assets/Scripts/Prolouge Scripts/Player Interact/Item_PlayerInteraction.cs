using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Item_PlayerInteraction : MonoBehaviour
{
    public float interactRange = 3f;
    public LayerMask interactLayer;
    public TMP_Text promptTextUI;

    private ItemInteractable currentInteractable;

    void Update()
    {
        CheckForInteractable();

        if (currentInteractable != null && Input.GetKeyDown(KeyCode.E))
        {
            currentInteractable.Interact();
        }
    }

    void CheckForInteractable()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange, interactLayer))
        {
            ItemInteractable interactable = hit.collider.GetComponent<ItemInteractable>();

            if (interactable != null)
            {
                currentInteractable = interactable;

                promptTextUI.text = interactable.promptText;
                promptTextUI.gameObject.SetActive(true);
                return;
            }
        }

        currentInteractable = null;
        promptTextUI.gameObject.SetActive(false);
    }
}

