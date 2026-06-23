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
    private CanvasGroup mainCanvasGroup;
    private BattleUnit playerUnit; 
    private bool isExecutingAction = false;
    private float lastClickTime = 0f;
    private string lastClickedAction = "";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        mainCanvasGroup = GetComponent<CanvasGroup>();
        if (mainCanvasGroup == null)
        {
            mainCanvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void OnEnable()
    {
        if (mainCanvasGroup != null)
        {
            mainCanvasGroup.alpha = 1f;
            mainCanvasGroup.interactable = true;
            mainCanvasGroup.blocksRaycasts = true;
        }
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
            // If no item in inventory, then display warning text UI
            if (InventoryManager.Instance == null || InventoryManager.Instance.GetInventoryList().Count == 0)
            {
                if (WarningMessageUI.Instance != null)
                {
                    WarningMessageUI.Instance.ShowWarning("You doesn't have any item");
                }
                return;
            }

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
        if (isExecutingAction) return;
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

    private Transform GetEnemyTransform()
    {
        GameObject enemyObj = GameObject.FindGameObjectWithTag("Enemy");
        return enemyObj != null ? enemyObj.transform : null;
    }

    private BattleUnit GetEnemyUnit()
    {
        GameObject enemyObj = GameObject.FindGameObjectWithTag("Enemy");
        return enemyObj != null ? enemyObj.GetComponent<BattleUnit>() : null;
    }

    private void ExecuteAction(BattleActionSlot slot)
    {
        if (playerUnit == null) return;

        lastClickedAction = "";
        lastClickTime = 0f;
        isExecutingAction = true;

        // Consume any required attribute resource costs before starting
        playerUnit.ConsumeCost(slot.costType, slot.costAmount);

        UnitAnimator playerAnimator = playerUnit.GetComponent<UnitAnimator>();
        Transform enemyTransform = GetEnemyTransform();
        BattleUnit enemyUnit = GetEnemyUnit();

        // Evaluate specific category behaviors
        if (slot.categoryName == "Attack")
        {
            HideMenuUI();
            CloseCurrentSubMenu();

            if (playerAnimator != null && enemyTransform != null)
            {
                int damage = Mathf.Max(1, playerUnit.strength - (enemyUnit != null ? enemyUnit.defense : 0));
                
                StartCoroutine(playerAnimator.MeleeAttackRoutine(enemyTransform, () => 
                {
                    if (enemyUnit != null) enemyUnit.TakeDamage(damage);
                }));

                StartCoroutine(WaitAndEndTurn(1.5f)); 
            }
            else EndPlayerTurnSequence();
        }
        else if (slot.categoryName == "Skill")
        {
            HideMenuUI();
            CloseCurrentSubMenu();

            if (playerAnimator != null && enemyTransform != null)
            {
                int damage = Mathf.Max(1, playerUnit.intelligence - (enemyUnit != null ? enemyUnit.resistance : 0));
                GameObject fireballPrefab = Resources.Load<GameObject>("Fireball"); 
                
                if (fireballPrefab != null)
                {
                    StartCoroutine(playerAnimator.RangedAttackRoutine(enemyTransform, fireballPrefab, () => 
                    {
                        if (enemyUnit != null) enemyUnit.TakeDamage(damage);
                    }));

                    StartCoroutine(WaitAndEndTurn(1.5f));
                }
                else
                {
                    Debug.LogError("[Action Menu] Prefab 'Fireball' not found");
                    EndPlayerTurnSequence();
                }
            }
            else EndPlayerTurnSequence();
        }
        else if (slot.categoryName == "Defend")
        {
            HideMenuUI();
            CloseCurrentSubMenu();

            playerUnit.SetDefend(true);
            EndPlayerTurnSequence(); 
        }
        else if (slot.categoryName == "Item" && slot.linkedItem != null)
        {
            ItemData item = slot.linkedItem;
            
            // Apply healing or temporary attribute buffs 
            playerUnit.UseItem(item);

            if (InventoryManager.Instance != null) InventoryManager.Instance.ConsumeItem(item);

            // Update UI list slots elements
            if (activeSubMenu != null)
            {
                RefreshItemSubMenu(activeSubMenu.transform);
            }
            else 
            {
                RefreshItemSubMenu(slot.transform.parent);
            }

            StartCoroutine(ItemCooldownRoutine());
        }
    }

    public void ShowMenuUI()
    {
        isExecutingAction = false;
        if (mainCanvasGroup != null)
        {
            mainCanvasGroup.alpha = 1f;
            mainCanvasGroup.interactable = true;
            mainCanvasGroup.blocksRaycasts = true;
        }
    }

    private void HideMenuUI()
    {
        if (mainCanvasGroup != null)
        {
            mainCanvasGroup.alpha = 0f;
            mainCanvasGroup.interactable = false;
            mainCanvasGroup.blocksRaycasts = false;
        }
    }

    private IEnumerator WaitAndEndTurn(float delay)
    {
        yield return new WaitForSeconds(delay);
        EndPlayerTurnSequence();
    }

    private void EndPlayerTurnSequence()
    {
        Debug.Log("[Action Menu] Turn completed. Passing authority to ENEMYTURN state...");
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.ChangeState(BattleState.ENEMYTURN);
        }

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.ChangeState(BattleState.ENEMYTURN);
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

    private IEnumerator ItemCooldownRoutine()
    {
        // 0.5 second buffer to prevent click spam in Item
        yield return new WaitForSeconds(0.5f);
        isExecutingAction = false;
    }
}