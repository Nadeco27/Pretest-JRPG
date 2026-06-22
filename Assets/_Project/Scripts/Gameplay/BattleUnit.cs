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

    [Header("Unit Attributes")]
    [Tooltip("Strength: Affects physical attack damage.")]
    public int strength;
    
    [Tooltip("Defense: Affects damage suffered from physical attacks.")]
    public int defense;
    
    [Tooltip("Intelligence: Affects magic attack damage.")]
    public int intelligence;
    
    [Tooltip("Resistance: Affects damage suffered from magic attacks.")]
    public int resistance;

    [Header("Visual UI Connection")]
    [Tooltip("Assign the corresponding HUD script for this unit.")]
    [SerializeField] private BattleHUD unitHUD;

    [Header("Defend System")]
    public bool isDefending = false;
    [SerializeField] private GameObject shieldVFXObject;

    private void Start()
    {
        UpdateVisualHUD();
        if (shieldVFXObject != null) shieldVFXObject.SetActive(false); 
    }

    public void TakeDamage(int damage)
    {
        // Damage mitigation logic (50% reduction from Defend Stance)
        if (isDefending)
        {
            int originalDamage = damage;
            damage = Mathf.CeilToInt(damage * 0.5f);
        }

        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        Debug.Log($"[{unitName}] Took {damage} damage! Remaining HP: {currentHP}");

        UpdateVisualHUD();

        if (!isPlayerUnit && BattleCutsceneManager.Instance != null)
        {
            BattleCutsceneManager.Instance.CheckEnemyHealthThreshold(currentHP);
        }

        if (currentHP <= 0)
        {
            Die();
        }
    }

    public void SetDefend(bool state)
    {
        isDefending = state;
        if (shieldVFXObject != null)
        {
            shieldVFXObject.SetActive(state);
        }
    }

    public void ConsumeCost(ActionCostType costType, int amount)
    {
        if (costType == ActionCostType.MP)
        {
            currentMP -= amount;
            currentMP = Mathf.Clamp(currentMP, 0, maxMP);
        }
        else if (costType == ActionCostType.HP)
        {
            currentHP -= amount;
            currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        }
        UpdateVisualHUD();
    }

    public void UpdateVisualHUD()
    {
        if (unitHUD != null)
        {
            unitHUD.UpdateHUD(currentHP, maxHP, currentMP, maxMP);
            unitHUD.UpdateAttributes(strength, defense, intelligence, resistance);
        }
    }

    public void UseItem(ItemData item)
    {
        // Heal Health
        if (item.healHP > 0)
        {
            currentHP = Mathf.Clamp(currentHP + item.healHP, 0, maxHP);
            if (unitHUD != null) unitHUD.TriggerStatHighlight("HP");
        }
        
        // Restore Mana
        if (item.restoreMP > 0)
        {
            currentMP = Mathf.Clamp(currentMP + item.restoreMP, 0, maxMP);
            if (unitHUD != null) unitHUD.TriggerStatHighlight("MP");
        }

        // Stat Buffs
        if (item.buffSTR > 0)
        {
            strength += item.buffSTR;
            if (unitHUD != null) unitHUD.TriggerStatHighlight("STR");
        }
        if (item.buffDEF > 0)
        {
            defense += item.buffDEF;
            if (unitHUD != null) unitHUD.TriggerStatHighlight("DEF");
        }
        if (item.buffINT > 0)
        {
            intelligence += item.buffINT;
            if (unitHUD != null) unitHUD.TriggerStatHighlight("INT");
        }
        if (item.buffRES > 0)
        {
            resistance += item.buffRES;
            if (unitHUD != null) unitHUD.TriggerStatHighlight("RES");
        }

        // Force UI numbers to update
        UpdateVisualHUD();
        Debug.Log($"[{unitName}] Used {item.itemName}. Stats updated!");
    }

    private void Die()
    {
        Debug.Log($"[{unitName}] Has been defeated!");
    }
}