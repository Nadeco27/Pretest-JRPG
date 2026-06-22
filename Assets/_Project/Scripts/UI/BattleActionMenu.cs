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
        public string categoryName; // "Attack", "Skill", atau "Item"
        public Button mainButton;
        public CanvasGroup subMenuCanvasGroup;
    }

    [Header("Menu Configuration")]
    [SerializeField] private List<ActionCategory> categories = new List<ActionCategory>();
    [SerializeField] private Button defendButton;

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float doubleClickThreshold = 0.3f;

    private CanvasGroup activeSubMenu;
    private float lastClickTime = 0f;
    private string lastClickedAction = "";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        foreach (var category in categories)
        {
            if (category.subMenuCanvasGroup != null)
            {
                category.subMenuCanvasGroup.alpha = 0f;
                category.subMenuCanvasGroup.interactable = false;
                category.subMenuCanvasGroup.blocksRaycasts = false;
            }

            // Assign click function to main action menu
            ActionCategory localCategory = category;
            if (localCategory.mainButton != null)
            {
                localCategory.mainButton.onClick.AddListener(() => OnMainCategoryClicked(localCategory));
            }
        }

        // Assign click function to defend action
        if (defendButton != null)
        {
            defendButton.onClick.AddListener(OnDefendClicked);
        }
    }

    // Detect right click to close menu
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

        // If any sub menu opened, close that submenu
        if (activeSubMenu != null && activeSubMenu != category.subMenuCanvasGroup)
        {
            SafeFade(activeSubMenu, 0f, false);
        }

        // Open new sub menu
        activeSubMenu = category.subMenuCanvasGroup;
        if (activeSubMenu != null)
        {
            SafeFade(activeSubMenu, 1f, true);
        }
    }

    public void ReceiveActionClick(string actionName, string categoryName)
    {
        if (BattleManager.Instance == null || BattleManager.Instance.state != BattleState.PLAYERTURN) return;

        float timeSinceLastClick = Time.time - lastClickTime;

        // Check double click
        if (timeSinceLastClick <= doubleClickThreshold && lastClickedAction == actionName)
        {
            ExecuteAction(actionName, categoryName);
        }
        else
        {
            lastClickTime = Time.time;
            lastClickedAction = actionName;
            Debug.Log($"[Action Menu] Single click detected on: {actionName}. Click again to execute.");
        }
    }

    private void OnDefendClicked()
    {
        ReceiveActionClick("Defend", "Defend");
    }

    private void ExecuteAction(string actionName, string categoryName)
    {
        Debug.Log($"[BATTLE EXECUTION] SUCCESS DOUBLE CLICK! Action: {actionName} (Cetegory: {categoryName})");

        CloseCurrentSubMenu();

        // Action economy
        if (categoryName == "Item")
        {
            Debug.Log("[Action Menu] Using item doesn't exhaust turn");
            return; 
        }

        EndPlayerTurnSequence();
    }

    private void EndPlayerTurnSequence()
    {
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