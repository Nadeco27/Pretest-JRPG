using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BackpackUI : MonoBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField] private int maxSlots = 5;

    [Header("Spawning Grid")]
    [SerializeField] private Transform slotContainer;
    [SerializeField] private InventorySlotUI slotPrefab;

    [Header("Red Description Panel Components")]
    [SerializeField] private GameObject descriptionPanel;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemAmountText;
    [SerializeField] private TextMeshProUGUI itemDescText;

    private CanvasGroup canvasGroup;
    private bool isOpen = false;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        HideBackpack();
    }

    // Completely updates the grid layout based on current inventory data
    public void RefreshInventoryUI(List<InventoryItem> items)
    {
        foreach (Transform child in slotContainer)
        {
            Destroy(child.gameObject);
        }

        // Spawn slots. Items are drawn in list order (newest will be leftmost)
        for (int i = 0; i < maxSlots; i++)
        {
            InventorySlotUI newSlot = Instantiate(slotPrefab, slotContainer);

            // If item index fewer then owned item tipe, fill the slots
            if (i < items.Count)
            {
                newSlot.SetupSlot(items[i].data, items[i].quantity, this);
            }
            else
            {
                newSlot.SetupEmptySlot();
            }
        }
    }

    // Invoked by individual slots to turn on the description box
    public void ShowItemDescription(ItemData item, int amount)
    {
        descriptionPanel.SetActive(true);
        itemNameText.text = item.itemName;
        itemAmountText.text = $"Amount: {amount}";
        itemDescText.text = item.itemDescription;
    }

    public void ToggleBackpack(List<InventoryItem> items)
    {
        if (isOpen) HideBackpack();
        else ShowBackpack(items);
    }

    public void ShowBackpack(List<InventoryItem> items)
    {
        isOpen = true;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        descriptionPanel.SetActive(false);
        RefreshInventoryUI(items);
    }

    public void HideBackpack()
    {
        isOpen = false;
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }
    
    public bool IsOpen() => isOpen;
}