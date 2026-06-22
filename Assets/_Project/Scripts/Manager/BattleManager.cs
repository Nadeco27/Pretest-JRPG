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
        state = BattleState.START;
        StartCoroutine(SetupBattle());
    }

    private IEnumerator SetupBattle()
    {
        // Set state to START initially
        state = BattleState.START;

        // Intro diialogue
        if (BattleCutsceneManager.Instance != null)
        {
            state = BattleState.BUSY_CUTSCENE; 
            yield return StartCoroutine(BattleCutsceneManager.Instance.ExecuteBattleCutscene("Intro_Battle"));
        }

        // Tutorial panel
        if (BattleTutorialManager.Instance != null)
        {
            yield return StartCoroutine(BattleTutorialManager.Instance.ShowTutorialRoutine());
        }

        if (BattleUIController.Instance != null)
        {
            BattleUIController.Instance.ShowBattleUI();
        }

        state = BattleState.PLAYERTURN;
        PlayerTurn();
    }

    public void PlayerTurn()
    {
        state = BattleState.PLAYERTURN;
        
        if (diamondActionMenuParent != null)
        {
            diamondActionMenuParent.SetActive(true);
        }
    }

    public void SetBattleState(BattleState newState)
    {
        state = newState;
    }
}