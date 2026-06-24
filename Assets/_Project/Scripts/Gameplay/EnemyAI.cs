using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BattleUnit))]
[RequireComponent(typeof(UnitAnimator))]
public class EnemyAI : MonoBehaviour
{
    private BattleUnit myUnit;
    private UnitAnimator myAnimator;
    private BattleUnit playerUnit;

    [Header("AI Intelligence Settings")]
    [Tooltip("Time AI to think to decide action")]
    [SerializeField] private float thinkingDelay = 1.5f;

    [Header("Available Moves Pool")]
    [Tooltip("Put all ActionData enemy can to do action here")]
    [SerializeField] private List<ActionData> availableActions = new List<ActionData>();

    private void Awake()
    {
        myUnit = GetComponent<BattleUnit>();
        myAnimator = GetComponent<UnitAnimator>();
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerUnit = playerObj.GetComponent<BattleUnit>();
    }

    // Main function BattleManager called when Enemy turn
    public void TakeTurn()
    {
        if (myUnit.currentHP <= 0) return;
        StartCoroutine(DecisionRoutine());
    }

    private IEnumerator DecisionRoutine()
    {
        Debug.Log($"[{myUnit.unitName} AI] Analyzing combat situation...");
        float timer = 0f;
        while (timer < thinkingDelay)
        {
            // If State suddenly change to BUSY because of dialogue Fungus, stop action routine and wat for turn
            while (BattleManager.Instance != null && BattleManager.Instance.state == BattleState.BUSY_CUTSCENE)
            {
                yield return null;
            }

            timer += Time.deltaTime;
            yield return null; 
        }

        // Filter valid action (can afford health or mana cost)
        List<ActionData> affordableActions = new List<ActionData>();
        foreach (var action in availableActions)
        {
            if (CanAffordAction(action))
            {
                affordableActions.Add(action);
            }
        }

        // Defensive stance
        // When health below 25%, 50% chance to do Defend
        float hpPercentage = (float)myUnit.currentHP / myUnit.maxHP;
        if (hpPercentage < 0.25f && Random.value < 0.5f)
        {
            ExecuteDefend();
            yield break;
        }

        // Take decision offensive action
        if (affordableActions.Count > 0)
        {
            // Choose random action from afforable action
            ActionData chosenAction = affordableActions[Random.Range(0, affordableActions.Count)];
            StartCoroutine(ExecuteChosenActionRoutine(chosenAction));
        }
        else
        {
            // If no affordable action, do defense
            Debug.LogWarning($"[{myUnit.unitName} AI] Cannot afford any action! Defaulting to Defend.");
            ExecuteDefend();
        }
    }

    private bool CanAffordAction(ActionData action)
    {
        if (action.costType == ActionCostType.None) return true;
        if (action.costType == ActionCostType.HP) return myUnit.currentHP > action.costAmount;
        if (action.costType == ActionCostType.MP) return myUnit.currentMP >= action.costAmount;
        return false;
    }

    private IEnumerator ExecuteChosenActionRoutine(ActionData action)
    {
        Debug.Log($"[{myUnit.unitName} AI] Decided to cast: {action.actionName}");

        if (action.animationType == ActionAnimationType.Melee)
        {
            yield return StartCoroutine(myAnimator.MeleeAttackRoutine(playerUnit.transform, () => 
            {
                if (playerUnit != null) myUnit.ExecuteActionEffects(action, playerUnit);
            }));
        }
        else // Ranged Animation
        {
            GameObject projectile = action.projectilePrefab != null ? action.projectilePrefab : Resources.Load<GameObject>("EnemyFireball");

            yield return StartCoroutine(myAnimator.RangedAttackRoutine(playerUnit.transform, projectile, () => 
            {
                if (playerUnit != null) myUnit.ExecuteActionEffects(action, playerUnit);
            }));
        }

        EndTurn();
    }

    private void ExecuteDefend()
    {
        Debug.Log($"[{myUnit.unitName} AI] Decided to stand ground: DEFEND");
        myUnit.SetDefend(true);
        EndTurn();
    }

    private void EndTurn()
    {
        Debug.Log($"[{myUnit.unitName} AI] Turn action completed. Handing over authority...");
        
        if (BattleInfoPanel.Instance != null) BattleInfoPanel.Instance.AdvanceCycle();

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.ChangeState(BattleState.PLAYERTURN);
        }
    }
}