using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Enum for defining cost requirements
public enum ActionCostType { None, HP, MP }

[RequireComponent(typeof(Button))]
// Implementing interfaces to detect mouse hover enter and exit
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

    private Button btn;
    private BattleUnit playerUnit; // Cached reference

    private void Start()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnSlotClicked);

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerUnit = playerObj.GetComponent<BattleUnit>();
        }
    }

    // Triggered automatically when mouse pointer hovers over the button
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (DynamicTooltip.Instance != null)
        {
            string costStr = costType == ActionCostType.None ? "Cost: Free" : $"Cost: {costAmount} {costType}";
            DynamicTooltip.Instance.ShowTooltip(actionName, costStr, actionDescription);
        }
    }

    // Triggered automatically when mouse pointer leaves the button
    public void OnPointerExit(PointerEventData eventData)
    {
        if (DynamicTooltip.Instance != null) DynamicTooltip.Instance.HideTooltip();
    }

    private void OnDisable()
    {
        // Hide tooltip if menu is suddenly destroyed or disabled
        if (DynamicTooltip.Instance != null) DynamicTooltip.Instance.HideTooltip();
    }

    private void OnSlotClicked()
    {
        // Cost validation check
        if (!HasEnoughStats())
        {
            Debug.LogWarning($"[Action Slot] Insufficient {costType} to execute {actionName}!");
            if (WarningMessageUI.Instance != null)
            {
                WarningMessageUI.Instance.ShowWarning($"Not enough {costType} to use {actionName}!");
            }
            return;
        }

        // 2. Consume stats if this is a double-click actual execution
        // Note: For a real double-click system, you might want to deduct cost INSIDE BattleActionMenu ExecuteAction.
        // But for prototype simplicity, we assume action is valid. 
        // We will let the BattleActionMenu handle the actual state, but we send the click forward.
        
        if (BattleActionMenu.Instance != null)
        {
            BattleActionMenu.Instance.ReceiveActionClick(actionName, categoryName);
        }
    }

    private bool HasEnoughStats()
    {
        if (costType == ActionCostType.None) return true;
        if (playerUnit == null) return false;

        // HP cost must not kill the player
        if (costType == ActionCostType.HP) return playerUnit.currentHP > costAmount;
        // MP cost just needs to be sufficient
        if (costType == ActionCostType.MP) return playerUnit.currentMP >= costAmount;
        
        return true;
    }
}