using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(Button))]
public class BattleActionSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Modular Action Data")]
    public ActionData linkedAction; 

    [Header("Category & Item Fallback")]
    public string categoryName;
    [HideInInspector] public ItemData linkedItem;
    [HideInInspector] public InventoryItem storedInventoryItem;

    private Button btn;
    private BattleUnit playerUnit;
    private TextMeshProUGUI buttonText;

    private void Awake()
    {
        btn = GetComponent<Button>();
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
        
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnSlotClicked);
    }

    private void Start()
    {
        FindPlayerUnit();

        if (linkedAction != null && buttonText != null)
        {
            buttonText.text = linkedAction.actionName;
        }
    }

    private void FindPlayerUnit()
    {
        if (playerUnit == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerUnit = playerObj.GetComponent<BattleUnit>();
        }
    }

    public void SetupAsItem(InventoryItem invItem)
    {
        categoryName = "Item";
        linkedAction = null;
        linkedItem = invItem.data;
        storedInventoryItem = invItem; 
        
        if (buttonText != null) buttonText.text = linkedItem.itemName;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        RefreshTooltipUI();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (DynamicTooltip.Instance != null) DynamicTooltip.Instance.HideTooltip();
    }

    private void OnDisable()
    {
        if (DynamicTooltip.Instance != null) DynamicTooltip.Instance.HideTooltip();
    }

    private void OnSlotClicked()
    {
        if (!HasEnoughStats())
        {
            if (WarningMessageUI.Instance != null) WarningMessageUI.Instance.ShowWarning("Cannot afford cost of action");
            return; 
        }
        
        if (BattleActionMenu.Instance != null) 
        {
            BattleActionMenu.Instance.ReceiveActionClick(this);
        }

        if (gameObject.activeInHierarchy)
        {
            RefreshTooltipUI();
        }
        else
        {
            if (DynamicTooltip.Instance != null) DynamicTooltip.Instance.HideTooltip();
        }
    }

    private void RefreshTooltipUI()
    {
        // Refresh and automatically assign text to tooltip based on type
        if (DynamicTooltip.Instance != null)
        {
            string nameToShow = "";
            string costStr = "";
            string descToShow = "";

            if (categoryName == "Item" && linkedItem != null)
            {
                nameToShow = linkedItem.itemName;
                int currentQty = storedInventoryItem != null ? storedInventoryItem.quantity : 0;
                costStr = $"Quantity: {currentQty}";
                descToShow = linkedItem.itemDescription; 
            }
            else if (linkedAction != null)
            {
                nameToShow = linkedAction.actionName;
                costStr = linkedAction.costType == ActionCostType.None ? "Cost: Free" : $"Cost: {linkedAction.costAmount} {linkedAction.costType}";
                descToShow = linkedAction.description;
            }
            else if (categoryName == "Defend")
            {
                nameToShow = "Defend";
                costStr = "Cost: Free";
                descToShow = "Reduce incoming damage by 50% for 1 turn.";
            }

            DynamicTooltip.Instance.ShowTooltip(nameToShow, costStr, descToShow);
        }
    }

    private bool HasEnoughStats()
    {
        if (categoryName == "Item") 
        {
            int currentQty = storedInventoryItem != null ? storedInventoryItem.quantity : 0;
            return currentQty > 0;
        }

        if (linkedAction == null) return true; 
        if (linkedAction.costType == ActionCostType.None) return true;

        FindPlayerUnit();
        if (playerUnit == null) return false; 

        if (linkedAction.costType == ActionCostType.HP) return playerUnit.currentHP > linkedAction.costAmount;
        if (linkedAction.costType == ActionCostType.MP) return playerUnit.currentMP >= linkedAction.costAmount;

        return true;
    }

    // Make sure Item identity read as the real name, not as just a string
    public string actionName 
    { 
        get 
        { 
            if (categoryName == "Item" && linkedItem != null) return linkedItem.itemName;
            return linkedAction != null ? linkedAction.actionName : categoryName; 
        } 
        set {} 
    }
}