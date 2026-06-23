using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public enum ActionCostType { None, HP, MP }

[RequireComponent(typeof(Button))]
public class BattleActionSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Action Identifiers")]
    public string categoryName;
    public string actionName;
    
    [Header("Tooltip Data")]
    [TextArea(2, 4)] public string actionDescription;
    
    [Header("Cost Requirements")]
    public ActionCostType costType;
    public int costAmount;

    [HideInInspector] public ItemData linkedItem;
    [HideInInspector] public int itemQuantity;

    private Button btn;
    private BattleUnit playerUnit;
    private TextMeshProUGUI buttonText;

    private void Awake()
    {
        btn = GetComponent<Button>();
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Start()
    {
        btn.onClick.AddListener(OnSlotClicked);

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerUnit = playerObj.GetComponent<BattleUnit>();
    }

    public void SetupAsItem(InventoryItem invItem)
    {
        categoryName = "Item";
        linkedItem = invItem.data;
        actionName = linkedItem.itemName;
        actionDescription = linkedItem.itemDescription;
        itemQuantity = invItem.quantity;
        
        // Items generally don't cost HP/MP to use
        costType = ActionCostType.None; 
        costAmount = 0;

        // Change the actual text on the button
        if (buttonText != null) buttonText.text = actionName;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (DynamicTooltip.Instance != null)
        {
            string costStr = "";
            
            // Format tooltip differently if it's an Item vs a Skill
            if (categoryName == "Item") 
                costStr = $"Quantity: {itemQuantity}";
            else 
                costStr = costType == ActionCostType.None ? "Cost: Free" : $"Cost: {costAmount} {costType}";

            DynamicTooltip.Instance.ShowTooltip(actionName, costStr, actionDescription);
        }
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
            if (WarningMessageUI.Instance != null)
            {
                WarningMessageUI.Instance.ShowWarning("Cannot afford cost of action");
            }
            return;
        }
        
        if (BattleActionMenu.Instance != null) BattleActionMenu.Instance.ReceiveActionClick(this);
    }

    private bool HasEnoughStats()
    {
        if (costType == ActionCostType.None) return true;
        if (playerUnit == null) return false; 

        if (costType == ActionCostType.HP) return playerUnit.currentHP > costAmount;
        if (costType == ActionCostType.MP) return playerUnit.currentMP >= costAmount;
        
        return true;
    }
}