using UnityEngine;

// Ensure this filename is ObjectInteractor.cs
public class ObjectInteractor : MonoBehaviour
{
    public float interactDistance = 3.5f;
    public KeypadSystem keypadSystem;

    void Update()
    {
        // Draw a line in the Scene view to help you see the interaction range
        Debug.DrawRay(transform.position, transform.forward * interactDistance, Color.green);

        if (Input.GetKeyDown(KeyCode.E))
        {
            // Don't interact if the Keypad is already open
            if (keypadSystem.keypadUI.activeSelf) return;

            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactDistance))
            {
                // OPTION A: Keypad Interaction
                if (hit.collider.CompareTag("Interactable"))
                {
                    keypadSystem.ToggleKeypad(true);
                }

                // OPTION B: Display Label Interaction
                if (hit.collider.CompareTag("Label"))
                {
                    DisplayLabel label = hit.collider.GetComponent<DisplayLabel>();
                    if (label != null)
                    {
                        label.ShowDescription();
                    }
                }
            }
        }
    }
}