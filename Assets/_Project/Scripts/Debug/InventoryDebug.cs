using System.Collections.Generic;
using UnityEngine;

public class InventoryDebug : MonoBehaviour
{
    [System.Serializable]
    public class DebugItemConfig
    {
        public ItemData item;
        public int amount;
    }

    [Header("Debug Settings")]
    [Tooltip("Press this key during gameplay to inject items.")]
    public KeyCode debugKey = KeyCode.F1;
    
    [Tooltip("List of items to give to the player.")]
    public List<DebugItemConfig> itemsToInject = new List<DebugItemConfig>();

    private void Update()
    {
        // Only trigger when the specific debug key is pressed
        if (Input.GetKeyDown(debugKey))
        {
            InjectItems();
        }
    }

    private void InjectItems()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("[Inventory Debug] Failed! InventoryManager not found in the scene.");
            return;
        }

        foreach (var config in itemsToInject)
        {
            if (config.item != null && config.amount > 0)
            {
                InventoryManager.Instance.AddItemToInventory(config.item, config.amount);
                Debug.Log($"[Inventory Debug] Successfully added {config.amount}x {config.item.itemName} to inventory.");
            }
        }
    }
}