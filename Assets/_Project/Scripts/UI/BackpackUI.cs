using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BackpackUI : MonoBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField] private int maxSlots = 5;

    [Header("UI Component Link")]
    [SerializeField] private Button backpackButton;

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

    private void Start()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.RegisterBackpackUI(this);
        }

        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null && parentCanvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            parentCanvas.worldCamera = Camera.main;
        }

        // Programmatically assign button click to avoid missing reference bugs
        if (backpackButton != null)
        {
            backpackButton.onClick.RemoveAllListeners();
            backpackButton.onClick.AddListener(TriggerToggleFromManager);
        }
    }

    private void TriggerToggleFromManager()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.ToggleBackpackFromButton();
        }
    }

    public void RefreshInventoryUI(List<InventoryItem> items)
    {
        foreach (Transform child in slotContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < maxSlots; i++)
        {
            InventorySlotUI newSlot = Instantiate(slotPrefab, slotContainer);

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
        if (GetComponent<CanvasGroup>() == null) return;
        
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