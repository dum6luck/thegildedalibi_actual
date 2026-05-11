using UnityEngine;

// The filename MUST be ObjectInteractor.cs
public class ObjectInteractor : MonoBehaviour
{
    public float interactDistance = 3.5f;
    public KeypadSystem keypadSystem;

    void Update()
    {
        // Only check for 'E' if the keypad isn't already open
        if (Input.GetKeyDown(KeyCode.E) && !keypadSystem.keypadUI.activeSelf)
        {
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactDistance))
            {
                if (hit.collider.CompareTag("Interactable"))
                {
                    keypadSystem.ToggleKeypad(true);
                }
            }
        }
    }
}