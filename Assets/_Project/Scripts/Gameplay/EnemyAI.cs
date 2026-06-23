using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BattleUnit))]
[RequireComponent(typeof(UnitAnimator))]
public class EnemyAI : MonoBehaviour
{
    private BattleUnit myUnit;
    private UnitAnimator myAnimator;
    private BattleUnit playerUnit;

    [Header("AI Settings")]
    [Tooltip("Berapa MP yang dibutuhkan musuh untuk cast skill?")]
    [SerializeField] private int skillMPCost = 10;
    
    [Tooltip("Waktu AI diam 'berpikir' sebelum menyerang")]
    [SerializeField] private float thinkingDelay = 2.0f;

    private void Awake()
    {
        myUnit = GetComponent<BattleUnit>();
        myAnimator = GetComponent<UnitAnimator>();
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerUnit = playerObj.GetComponent<BattleUnit>();
        }
    }

    public void TakeTurn()
    {
        if (myUnit.currentHP <= 0) return;
        StartCoroutine(DecisionRoutine());
    }

    private IEnumerator DecisionRoutine()
    {
        Debug.Log("[Enemy AI] Enemy is thinking...");
        yield return new WaitForSeconds(thinkingDelay);

        // Contextual decision
        float hpPercentage = (float)myUnit.currentHP / myUnit.maxHP;
        bool hasEnoughMP = myUnit.currentMP >= skillMPCost;

        float randomRoll = Random.value; // Random 0.0 - 1.0

        if (hpPercentage < 0.25f && randomRoll < 0.7f)
        {
            // Health below 25% -> 70% chance to defense
            ExecuteDefend();
        }
        else if (hasEnoughMP && randomRoll < 0.6f)
        {
            // HP good and enough Mana = 60% chance to skill
            StartCoroutine(ExecuteSkill());
        }
        else
        {
            // Else, attack normally
            StartCoroutine(ExecuteAttack());
        }
    }

    private IEnumerator ExecuteAttack()
    {
        Debug.Log("[Enemy AI] Decided to: ATTACK");
        int damage = Mathf.Max(1, myUnit.strength - (playerUnit != null ? playerUnit.defense : 0));

        yield return StartCoroutine(myAnimator.MeleeAttackRoutine(playerUnit.transform, () => 
        {
            if (playerUnit != null) playerUnit.TakeDamage(damage);
        }));

        EndTurn();
    }

    private IEnumerator ExecuteSkill()
    {
        Debug.Log("[Enemy AI] Decided to: SKILL");
        myUnit.ConsumeCost(ActionCostType.MP, skillMPCost);

        int damage = Mathf.Max(1, myUnit.intelligence - (playerUnit != null ? playerUnit.resistance : 0));
        
        // Call fireball prefab to cast skill
        GameObject enemyProjectile = Resources.Load<GameObject>("EnemyFireball"); 

        if (enemyProjectile != null)
        {
            yield return StartCoroutine(myAnimator.RangedAttackRoutine(playerUnit.transform, enemyProjectile, () => 
            {
                if (playerUnit != null) playerUnit.TakeDamage(damage);
            }));
        }
        else
        {
            Debug.LogWarning("EnemyFireball prefab missing! Defaulting to Melee.");
            yield return StartCoroutine(ExecuteAttack());
        }

        EndTurn();
    }

    private void ExecuteDefend()
    {
        Debug.Log("[Enemy AI] Decided to: DEFEND");
        myUnit.SetDefend(true);
        EndTurn();
    }

    private void EndTurn()
    {
        Debug.Log("[Enemy AI] Turn finished. Passing to Player...");
        
        if (BattleInfoPanel.Instance != null)
        {
            BattleInfoPanel.Instance.AdvanceCycle();
        }

        // Give turn to player
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.ChangeState(BattleState.PLAYERTURN);
        }
    }
}