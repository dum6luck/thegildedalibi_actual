using UnityEngine;
using System.Collections.Generic;

public class UserInventory : MonoBehaviour
{
    public static UserInventory Instance { get; private set; }

    private HashSet<string> inventoryItems = new HashSet<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AddItem(string itemID)
    {
        if (!inventoryItems.Contains(itemID))
        {
            inventoryItems.Add(itemID);
            Debug.Log($"[UserInventory] Added item: {itemID}");
        }
    }

    public bool HasItem(string itemID)
    {
        return inventoryItems.Contains(itemID);
    }

    public bool RemoveItem(string itemID)
    {
        return inventoryItems.Remove(itemID);
    }
}