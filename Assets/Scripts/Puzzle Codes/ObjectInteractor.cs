using UnityEngine;

// The filename MUST be ObjectInteractor.cs
public class ObjectInteractor : MonoBehaviour
{
    public float interactDistance = 3.5f;
    public KeypadSystem keypadSystem;

    void Update()
    {
        // This draws a green line in your SCENE view so you can see the ray's reach
        Debug.DrawRay(transform.position, transform.forward * interactDistance, Color.green);

        if (Input.GetKeyDown(KeyCode.E) && !keypadSystem.keypadUI.activeSelf)
        {
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactDistance))
            {
                Debug.Log("Hit: " + hit.collider.name); // This tells you what you actually hit

                if (hit.collider.CompareTag("Interactable"))
                {
                    keypadSystem.ToggleKeypad(true);
                }
            }
        }
    }
}