using System.Collections.Generic;
using UnityEngine;
using Fungus;

// Serializable class to configure item and its weight
[System.Serializable]
public class LootDrop
{
    public ItemData item;
    
    [Tooltip("Higher weight means higher chance to drop relative to other items.")]
    public int weight = 1; 
}

[RequireComponent(typeof(Collider2D))]
public class CardboardInteractable : InteractableBase
{
    [Header("Loot Configuration")]
    [Tooltip("Exact number of items the player will pull from this box.")]
    [SerializeField] private int itemsToDrop = 1;
    [SerializeField] private List<LootDrop> possibleDrops = new List<LootDrop>();

    [Header("Fungus Integration")]
    [Tooltip("The Fungus Flowchart containing the empty dialog.")]
    [SerializeField] private Flowchart dialogFlowchart;
    [SerializeField] private string emptyBoxBlockName = "EmptyBox";

    private bool hasBeenLooted = false;

    public override void Interact()
    {
        if (hasBeenLooted) return;

        hasBeenLooted = true;
        HidePrompt();

        AudioManager.Instance.Play("CardboardOpen");

        // Disable the trigger collider so the player's radar no longer detects this box
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            if (col.isTrigger)
            {
                col.enabled = false;
                break;
            }
        }

        // Progress the optional tutorial objective
        ObjectiveManager.Instance.NotifyObjectiveProgress("InteractTutorial");

        if (itemsToDrop <= 0 || possibleDrops.Count == 0)
        {
            HandleEmptyBox();
            return;
        }

        GenerateLoot();
    }

    private void HandleEmptyBox()
    {
        // Trigger Fungus block to show dialog
        if (dialogFlowchart != null && dialogFlowchart.HasBlock(emptyBoxBlockName))
        {
            dialogFlowchart.ExecuteBlock(emptyBoxBlockName);
        }
        else
        {
            Debug.LogWarning("Fungus Flowchart or Block is missing for the empty box!");
        }
    }

    private void GenerateLoot()
    {
        int totalWeight = 0;
        foreach (var drop in possibleDrops) totalWeight += drop.weight;

        // Dictionary to aggregate matching items before sending to UI
        Dictionary<ItemData, int> droppedItems = new Dictionary<ItemData, int>();

        for (int i = 0; i < itemsToDrop; i++)
        {
            int randomPick = Random.Range(0, totalWeight);
            int currentWeightSum = 0;

            foreach (var drop in possibleDrops)
            {
                currentWeightSum += drop.weight;
                if (randomPick < currentWeightSum)
                {
                    // If the item type already dropped this interaction, increase the amount
                    if (droppedItems.ContainsKey(drop.item))
                    {
                        droppedItems[drop.item]++;
                    }
                    else
                    {
                        droppedItems.Add(drop.item, 1);
                    }
                    break;
                }
            }
        }

        // Loop through the aggregated dictionary and fire UI notifications
        foreach (var kvp in droppedItems)
        {
            Debug.Log($"Player received: {kvp.Key.itemName} x{kvp.Value}");
            NotificationManager.Instance.ShowItemNotification(kvp.Key, kvp.Value);

            // Push the item data structural values directly into the backpack database
            InventoryManager.Instance.AddItemToInventory(kvp.Key, kvp.Value);
        }
    }
}