using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public bool hasDisplayCaseKey = false;

    public void PickUpKey()
    {
        hasDisplayCaseKey = true;
        Debug.Log("[Inventory] Key automatically added to inventory from keypad unlock!");
    }
}