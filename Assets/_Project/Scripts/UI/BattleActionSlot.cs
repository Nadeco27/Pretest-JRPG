using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(Button))]
public class BattleActionSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Modular Action Data")]
    public ActionData linkedAction; 

    [Header("Category & Item Fallback")]
    public string categoryName;
    [HideInInspector] public ItemData linkedItem;
    [HideInInspector] public InventoryItem storedInventoryItem;

    [Header("Sub-Menu UI Juice Settings")]
    [SerializeField] private RectTransform textRectTransform;
    [SerializeField] private float textIndentOffset = 15f;
    [SerializeField] private float textAnimationSpeed = 0.1f;

    private Button btn;
    private BattleUnit playerUnit;
    private TextMeshProUGUI buttonText;
    private Vector2 originalTextPosition;
    private Vector3 originalSlotScale;

    private void Awake()
    {
        originalSlotScale = transform.localScale;

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

        // Save button original position
        if (textRectTransform != null)
        {
            originalTextPosition = textRectTransform.anchoredPosition;
        }
        else if (buttonText != null)
        {
            textRectTransform = buttonText.GetComponent<RectTransform>();
            originalTextPosition = textRectTransform.anchoredPosition;
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

        if (btn != null && !btn.interactable) return;
        if (!HasEnoughStats()) return; 

        if (textRectTransform != null)
        {
            textRectTransform.DOKill();
            textRectTransform.DOAnchorPos(new Vector2(originalTextPosition.x + textIndentOffset, originalTextPosition.y), textAnimationSpeed).SetEase(Ease.OutCubic);
        }

        transform.DOKill();
        transform.DOScale(originalSlotScale * 1.05f, textAnimationSpeed).SetEase(Ease.OutQuad);

        if (AudioManager.Instance != null) AudioManager.Instance.Play("ButtonHover");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOKill();
        if (textRectTransform != null) textRectTransform.DOKill();

        // Set text and button to original position
        transform.DOScale(originalSlotScale, textAnimationSpeed).SetEase(Ease.OutQuad);
        if (textRectTransform != null)
        {
            textRectTransform.DOAnchorPos(originalTextPosition, textAnimationSpeed).SetEase(Ease.OutQuad);
        }

        if (DynamicTooltip.Instance != null) DynamicTooltip.Instance.HideTooltip();
    }

    private void OnDisable()
    {
        if (DynamicTooltip.Instance != null) DynamicTooltip.Instance.HideTooltip();
        transform.DOKill();
        if (textRectTransform != null) textRectTransform.DOKill();
        
        transform.localScale = originalSlotScale;
        if (textRectTransform != null && originalTextPosition != Vector2.zero) 
        {
            textRectTransform.anchoredPosition = originalTextPosition;
        }
    }

    private void OnSlotClicked()
    {
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);

        if (!HasEnoughStats())
        {
            if (WarningMessageUI.Instance != null) WarningMessageUI.Instance.ShowWarning("Cannot afford cost of action");
            return; 
        }

        transform.DOKill();
        transform.DOPunchScale(new Vector3(-0.05f, -0.05f, 0f), 0.1f, 5, 1f);

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