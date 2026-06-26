using System.Collections;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("UI System References")]
    [Tooltip("Assign parent of all action button")]
    [SerializeField] private GameObject diamondActionMenuParent;

    [Header("End Battle UI References")]
    [SerializeField] private GameObject endBattleScreenPanel;
    [SerializeField] private TMPro.TextMeshProUGUI resultTitleText;
    [SerializeField] private TMPro.TextMeshProUGUI finalTimeText;
    [SerializeField] private TMPro.TextMeshProUGUI finalCycleText;
    [SerializeField] private UnityEngine.UI.Button actionButton;
    [SerializeField] private TMPro.TextMeshProUGUI actionButtonText;
    [SerializeField] private float titleSlamdownDuration = 0.5f;
    [SerializeField] private float titleBounceDuration = 0.5f;

    public BattleState state;
    [HideInInspector] public BattleState pendingState;
    [HideInInspector] public bool hasPendingState = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartCoroutine(SetupBattle());
    }

    private IEnumerator SetupBattle()
    {
        // Save inventory before battle
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.SavePreBattleState();
        }

        // Initialize starting state
        state = BattleState.START;

        // Play intro dialogue
        if (BattleCutsceneManager.Instance != null)
        {
            yield return StartCoroutine(BattleCutsceneManager.Instance.ExecuteBattleCutscene("Intro_Battle"));
        }

        // Show tutorial panel
        if (BattleTutorialManager.Instance != null)
        {
            yield return StartCoroutine(BattleTutorialManager.Instance.ShowTutorialRoutine());
        }

        // Show UI battle
        if (BattleUIController.Instance != null)
        {
            BattleUIController.Instance.ShowBattleUI();
        }

        // Start player turn
        ChangeState(BattleState.PLAYERTURN);
    }

    public void ChangeState(BattleState newState, bool isResolvingQueue = false)
    {
        // State queue system
        if (state == BattleState.BUSY_CUTSCENE && !isResolvingQueue && 
            (newState == BattleState.PLAYERTURN || newState == BattleState.ENEMYTURN || newState == BattleState.WON || newState == BattleState.LOST))
        {
            Debug.Log($"[BattleManager] Saving {newState} to queue because cutscene is currently running");
            pendingState = newState;
            hasPendingState = true;
            return;
        }

        // Check status duplicate
        if (state == newState)
        {
            Debug.LogWarning($"[BattleManager] State is already {newState}. Ignoring duplicate request.");
            return; 
        }

        state = newState;
        Debug.Log($"[BattleManager] State changed to: {newState}");

        switch (state)
        {
            case BattleState.PLAYERTURN:
                SetupPlayerTurn();
                break;       
            case BattleState.ENEMYTURN:
                SetupEnemyTurn();
                break;                
            case BattleState.BUSY_CUTSCENE:
                HandleBusyCutsceneState();
                break;
            case BattleState.WON:
                StartCoroutine(HandleVictorySequence());
                break;
                
            case BattleState.LOST:
                StartCoroutine(HandleDefeatSequence());
                break;
        }
    }

    private void SetupPlayerTurn()
    {
        Debug.Log("Player turn started!");

        if (BattleInfoPanel.Instance != null)
        {
            BattleInfoPanel.Instance.SetTurnText("Player Turn", Color.cyan);
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            BattleUnit playerUnit = playerObj.GetComponent<BattleUnit>();
            if (playerUnit != null) playerUnit.ProcessTurnStart();
        }

        if (diamondActionMenuParent != null)
        {
            diamondActionMenuParent.SetActive(true);
        }

        if (BattleActionMenu.Instance != null)
        {
            BattleActionMenu.Instance.ShowMenuUI();
        }
    }

    private void SetupEnemyTurn()
    {
        Debug.Log("Enemy turn started!");

        if (BattleInfoPanel.Instance != null)
        {
            BattleInfoPanel.Instance.SetTurnText("Enemy Turn", Color.red);
        }

        GameObject enemyObj = GameObject.FindGameObjectWithTag("Enemy");
        if (enemyObj != null)
        {
            BattleUnit enemyUnit = enemyObj.GetComponent<BattleUnit>();
            EnemyAI enemyAI = enemyObj.GetComponent<EnemyAI>();

            if (enemyUnit != null) enemyUnit.ProcessTurnStart();

            if (enemyAI != null)
            {
                enemyAI.TakeTurn();
            }
            else
            {
                Debug.LogError("[BattleManager] EnemyAI component not found on Enemy!");
                ChangeState(BattleState.PLAYERTURN); 
            }
        }
    }

    private void HandleBusyCutsceneState()
    {
        Debug.Log("[BattleManager] Battle paused. Waiting for Fungus dialogue...");
        
        if (BattleActionMenu.Instance != null)
        {
            BattleActionMenu.Instance.HideMenuUI();
        }
        if (diamondActionMenuParent != null)
        {
            diamondActionMenuParent.SetActive(false);
        }
    }

    private IEnumerator HandleVictorySequence()
    {
        if (diamondActionMenuParent != null) diamondActionMenuParent.SetActive(false);
        if (BattleInfoPanel.Instance != null) BattleInfoPanel.Instance.StopTimer();

        AudioManager.Instance.Play("BattleWin");

        // Slow motion effect
        Time.timeScale = 0.3f;

        GameObject enemyObj = GameObject.FindGameObjectWithTag("Enemy");
        if (enemyObj != null)
        {
            UnitVisualController visualController = enemyObj.GetComponent<UnitVisualController>();
            if (visualController != null)
            {
                yield return StartCoroutine(visualController.FadeOutRoutine(0.5f));
            }
        }

        Time.timeScale = 1f;

        if (resultTitleText != null) resultTitleText.text = "YOU WIN";
        if (actionButtonText != null) actionButtonText.text = "Continue";

        PopulateEndScreenData();

        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(OnVictoryContinueClicked);

        if (endBattleScreenPanel != null) endBattleScreenPanel.SetActive(true);

        // Trigger UI animation
        if (resultTitleText != null)
        {
            StartCoroutine(AnimateTextBounce(resultTitleText.transform));
        }
    }

    private IEnumerator HandleDefeatSequence()
    {
        if (diamondActionMenuParent != null) diamondActionMenuParent.SetActive(false);
        if (BattleInfoPanel.Instance != null) BattleInfoPanel.Instance.StopTimer();

        // Slow motion effect
        Time.timeScale = 0.3f;
        yield return new WaitForSecondsRealtime(1.5f);

        AudioManager.Instance.Play("BattleLose");

        // Restore normal time scale
        Time.timeScale = 1f;

        if (resultTitleText != null) resultTitleText.text = "YOU LOSE";
        if (actionButtonText != null) actionButtonText.text = "Restart";

        PopulateEndScreenData();

        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(OnDefeatRestartClicked);

        if (endBattleScreenPanel != null) endBattleScreenPanel.SetActive(true);

        // Trigger UI animation
        if (resultTitleText != null)
        {
            StartCoroutine(AnimateTextBounce(resultTitleText.transform));
        }
    }

    private void PopulateEndScreenData()
    {
        if (BattleInfoPanel.Instance != null)
        {
            if (finalTimeText != null) 
                finalTimeText.text = $"Final Time: {BattleInfoPanel.Instance.GetCurrentTimeFormatted()}";
            if (finalCycleText != null) 
                finalCycleText.text = $"Total Cycles: {BattleInfoPanel.Instance.GetCurrentCycle()}";
        }
    }

    // Button event for winning screen
    private void OnVictoryContinueClicked()
    {
        PlayerPrefs.SetInt("DestroyLastEnemyTrigger", 1);

        if (ScreenTransitionManager.Instance != null)
        {
            ScreenTransitionManager.Instance.SwitchSceneWithTransition("Dream Scene");
        }
    }

    // Button event for losing screen
    private void OnDefeatRestartClicked()
    {
        Debug.Log("[BattleManager] Restarting Battle Scene...");

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.RestorePreBattleState();
        }

        if (ScreenTransitionManager.Instance != null)
        {
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            ScreenTransitionManager.Instance.SwitchSceneWithTransition(currentSceneName);
        }
    }

    private IEnumerator AnimateTextBounce(Transform textTransform)
    {
        float elapsedTime = 0f;
        Vector3 startScale = new Vector3(2f, 2f, 2f);
        Vector3 impactScale = new Vector3(0.8f, 0.8f, 0.8f);

        // Slam down effect
        while (elapsedTime < titleSlamdownDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            textTransform.localScale = Vector3.Lerp(startScale, impactScale, elapsedTime / titleSlamdownDuration);
            yield return null;
        }

        elapsedTime = 0f;
        Vector3 finalScale = Vector3.one;

        // Bounce back to normal scale
        while (elapsedTime < titleBounceDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            textTransform.localScale = Vector3.Lerp(impactScale, finalScale, elapsedTime / titleBounceDuration);
            yield return null;
        }

        textTransform.localScale = finalScale;
    }
}