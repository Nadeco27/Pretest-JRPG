using UnityEngine;

public class BattleUnit : MonoBehaviour
{
    [Header("Unit Information")]
    [Tooltip("The name of the unit displayed in UI.")]
    public string unitName;
    [Tooltip("Check if this unit is controlled by the player.")]
    public bool isPlayerUnit;

    [Header("Health Points (HP)")]
    public int maxHP;
    public int currentHP;

    [Header("Mana Points (MP)")]
    public int maxMP;
    public int currentMP;

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        Debug.Log($"[{unitName}] Took {damage} damage! Remaining HP: {currentHP}");

        if (!isPlayerUnit && BattleCutsceneManager.Instance != null)
        {
            BattleCutsceneManager.Instance.CheckEnemyHealthThreshold(currentHP);
        }

        if (currentHP <= 0)
        {
            Die();
        }
    }

    // Function to consume HP or MP for action costs
    public void ConsumeCost(ActionCostType costType, int amount)
    {
        if (costType == ActionCostType.MP)
        {
            currentMP -= amount;
            currentMP = Mathf.Clamp(currentMP, 0, maxMP);
            Debug.Log($"[{unitName}] Consumed {amount} MP. Remaining MP: {currentMP}");
        }
        else if (costType == ActionCostType.HP)
        {
            currentHP -= amount;
            currentHP = Mathf.Clamp(currentHP, 0, maxHP);
            Debug.Log($"[{unitName}] Consumed {amount} HP. Remaining HP: {currentHP}");
        }
    }

    private void Die()
    {
        Debug.Log($"[{unitName}] Has been defeated!");
    }
}