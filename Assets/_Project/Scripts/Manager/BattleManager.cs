using System.Collections;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [Header("UI System References")]
    [Tooltip("Assign parent of all action button")]
    [SerializeField] private GameObject diamondActionMenuParent;

    public static BattleManager Instance { get; private set; }

    public BattleState state;

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
        // Initialize starting state
        state = BattleState.START;

        // Play intro dialogue
        if (BattleCutsceneManager.Instance != null)
        {
            ChangeState(BattleState.BUSY_CUTSCENE); 
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

    public void ChangeState(BattleState newState)
    {
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
                // TODO: Call winning function
                break;
            case BattleState.LOST:
                // TODO: Call lose function
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
        
        if (diamondActionMenuParent != null)
        {
            diamondActionMenuParent.SetActive(false);
        }
    }
}