using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BattleActionMenu : MonoBehaviour, IPointerClickHandler
{
    public static BattleActionMenu Instance { get; private set; }

    [System.Serializable]
    public class ActionCategory
    {
        public string categoryName; // "Attack", "Skill", or "Item"
        public Button mainButton;   
        public CanvasGroup subMenuCanvasGroup; 
    }

    [Header("Menu Configuration")]
    [SerializeField] private List<ActionCategory> categories = new List<ActionCategory>();

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float doubleClickThreshold = 0.3f;

    private CanvasGroup activeSubMenu;
    private BattleUnit playerUnit; 
    private float lastClickTime = 0f;
    private string lastClickedAction = "";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerUnit = playerObj.GetComponent<BattleUnit>();
        }

        foreach (var category in categories)
        {
            if (category.subMenuCanvasGroup != null)
            {
                category.subMenuCanvasGroup.alpha = 0f;
                category.subMenuCanvasGroup.interactable = false;
                category.subMenuCanvasGroup.blocksRaycasts = false;
            }

            ActionCategory localCategory = category;
            if (localCategory.mainButton != null)
            {
                localCategory.mainButton.onClick.AddListener(() => OnMainCategoryClicked(localCategory));
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            CloseCurrentSubMenu();
        }
    }

    private void OnMainCategoryClicked(ActionCategory category)
    {
        if (BattleManager.Instance == null || BattleManager.Instance.state != BattleState.PLAYERTURN) return;

        // Populate items dynamically before opening
        if (category.categoryName == "Item")
        {
            RefreshItemSubMenu(category.subMenuCanvasGroup.transform);
        }

        if (activeSubMenu != null && activeSubMenu != category.subMenuCanvasGroup)
        {
            SafeFade(activeSubMenu, 0f, false);
        }

        activeSubMenu = category.subMenuCanvasGroup;
        if (activeSubMenu != null)
        {
            SafeFade(activeSubMenu, 1f, true);
        }
    }

    private void RefreshItemSubMenu(Transform container)
    {
        if (InventoryManager.Instance == null) return;

        var inventoryList = InventoryManager.Instance.GetInventoryList();
        BattleActionSlot[] slots = container.GetComponentsInChildren<BattleActionSlot>(true);

        for (int i = 0; i < slots.Length; i++)
        {
            if (i < inventoryList.Count)
            {
                slots[i].gameObject.SetActive(true);
                slots[i].SetupAsItem(inventoryList[i]);
            }
            else
            {
                slots[i].gameObject.SetActive(false);
            }
        }
    }

    public void ReceiveActionClick(BattleActionSlot slot)
    {
        if (BattleManager.Instance == null || BattleManager.Instance.state != BattleState.PLAYERTURN) return;

        float timeSinceLastClick = Time.time - lastClickTime;

        if (timeSinceLastClick <= doubleClickThreshold && lastClickedAction == slot.actionName)
        {
            ExecuteAction(slot);
        }
        else
        {
            lastClickTime = Time.time;
            lastClickedAction = slot.actionName;
            Debug.Log($"[Action Menu] Single click detected on: {slot.actionName}. Click again to execute.");
        }
    }

    private void ExecuteAction(BattleActionSlot slot)
    {
        Debug.Log($"[BATTLE EXECUTION] Executing: {slot.actionName} (Category: {slot.categoryName})");

        if (playerUnit != null)
        {
            // Consume stat costs
            playerUnit.ConsumeCost(slot.costType, slot.costAmount);

            // Evaluate specific category behaviors
            if (slot.categoryName == "Defend")
            {
                playerUnit.SetDefend(true);
            }
            else if (slot.categoryName == "Item" && slot.linkedItem != null)
            {
                ItemData item = slot.linkedItem;

                playerUnit.UseItem(item);

                // Consume from inventory
                if (InventoryManager.Instance != null) InventoryManager.Instance.ConsumeItem(item);

                // Refresh Item Menu immediately if it's still open
                RefreshItemSubMenu(activeSubMenu.transform);

                return; 
            }
        }

        // Close UI and pass turn for non-item actions
        CloseCurrentSubMenu();

        if (slot.categoryName != "Item")
        {
            EndPlayerTurnSequence();
        }
    }

    private void EndPlayerTurnSequence()
    {
        Debug.Log("[Action Menu] Turn completed. Passing authority to ENEMYTURN state...");
        gameObject.SetActive(false); 

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.SetBattleState(BattleState.ENEMYTURN);
        }
    }

    public void CloseCurrentSubMenu()
    {
        if (activeSubMenu != null)
        {
            SafeFade(activeSubMenu, 0f, false);
            activeSubMenu = null;
        }
    }

    private void SafeFade(CanvasGroup cg, float targetAlpha, bool interactable)
    {
        StartCoroutine(FadeRoutine(cg, targetAlpha, interactable));
    }

    private IEnumerator FadeRoutine(CanvasGroup cg, float targetAlpha, bool interactable)
    {
        float startAlpha = cg.alpha;
        float timeElapsed = 0f;

        if (!interactable)
        {
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }

        while (timeElapsed < fadeDuration)
        {
            timeElapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, timeElapsed / fadeDuration);
            yield return null;
        }

        cg.alpha = targetAlpha;
        if (interactable)
        {
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }
    }
}