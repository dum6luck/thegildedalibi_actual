using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInteractable : MonoBehaviour
{
    [TextArea] public string promptText = "Press I to Inspect";

    public virtual void Interact()
    {
        // This will be overridden by specific objects
        Debug.Log("Interacted with: " + gameObject.name);
    }
}
