using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI amountText;

    private ItemData currentItem;
    private int currentAmount;
    private BackpackUI backpackUI;

    // Injects data into the slot visualization
    public void SetupSlot(ItemData item, int amount, BackpackUI uiParent)
    {
        currentItem = item;
        currentAmount = amount;
        backpackUI = uiParent;

        itemIcon.gameObject.SetActive(true);
        itemIcon.sprite = item.itemIcon;
        amountText.text = amount.ToString();
        
        GetComponent<Button>().interactable = true;
        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(OnSlotClicked);
    }

    public void SetupEmptySlot()
    {
        currentItem = null;
        itemIcon.gameObject.SetActive(false);
        amountText.text = "";
        
        // Empty slot cannot be clicked
        GetComponent<Button>().interactable = false;
        GetComponent<Button>().onClick.RemoveAllListeners();
    }

    private void OnSlotClicked()
    {
        AudioManager.Instance.Play("ButtonClick");
        backpackUI.ShowItemDescription(currentItem, currentAmount);
    }
}