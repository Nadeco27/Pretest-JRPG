using UnityEngine;
using System.Collections.Generic;

public class BattleUnit : MonoBehaviour
{
    // List of temporary buff active
    [System.Serializable]
    public class ActiveBuff
    {
        public string buffName; 
        public int strAmount, defAmount, intAmount, resAmount;
        public int turnsRemaining;
    }

    [Header("Unit Information")]
    public string unitName;
    public bool isPlayerUnit;

    [Header("Health Points (HP)")]
    public int maxHP;
    public int currentHP;

    [Header("Mana Points (MP)")]
    public int maxMP;
    public int currentMP;

    // Seperate base stat with culculated bonus stat
    [Header("Base Attributes")]
    [Tooltip("Character base stat, value not gonna be changed in game")]
    public int baseStrength;
    public int baseDefense;
    public int baseIntelligence;
    public int baseResistance;

    [Header("Current Attributes (Calculated)")]
    [Tooltip("Fill with same number as base stat, this stat will be changed on game with buffs")]
    public int strength;
    public int defense;
    public int intelligence;
    public int resistance;

    [Header("Visual UI Connection")]
    [SerializeField] private BattleHUD unitHUD;

    [Header("Defend System")]
    public bool isDefending = false;

    [SerializeField] private GameObject shieldVFXObject;

    private List<ActiveBuff> activeBuffs = new List<ActiveBuff>();

    private void Awake()
    {
        baseStrength = strength;
        baseDefense = defense;
        baseIntelligence = intelligence;
        baseResistance = resistance;
        
        RecalculateStats();
    }

    private void Start()
    {
        UpdateVisualHUD();
        if (shieldVFXObject != null) shieldVFXObject.SetActive(false); 
    }

    public void RecalculateStats()
    {
        strength = baseStrength;
        defense = baseDefense;
        intelligence = baseIntelligence;
        resistance = baseResistance;

        // Add every buff in active buffs list
        foreach (var buff in activeBuffs)
        {
            strength += buff.strAmount;
            defense += buff.defAmount;
            intelligence += buff.intAmount;
            resistance += buff.resAmount;
        }

        UpdateVisualHUD();
    }

    public void TakeDamage(int damage)
    {

        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        Debug.Log($"[{unitName}] Took {damage} damage! Remaining HP: {currentHP}");

        UpdateVisualHUD();

        if (!isPlayerUnit && BattleCutsceneManager.Instance != null)
        {
            BattleCutsceneManager.Instance.CheckEnemyHealthThreshold(currentHP);
        }

        if (AudioManager.Instance != null)
        {
            if (isDefending)
            {
                AudioManager.Instance.Play("DefendDamage");
            }
            else
            {
                AudioManager.Instance.Play("TakeDamage");
            }
        }

        if (currentHP <= 0)
        {
            if (BattleManager.Instance != null)
            {
                if (isPlayerUnit)
                {
                    BattleManager.Instance.ChangeState(BattleState.LOST);
                }
                else
                {
                    BattleManager.Instance.ChangeState(BattleState.WON);
                }
            }
            else
            {
                Die();
            }
        }
    }

    public void SetDefend(bool state)
    {
        isDefending = state;
        if (shieldVFXObject != null)
        {
            shieldVFXObject.SetActive(state);
        }

        if (state == true) 
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.Play("DefendUse");
            }
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
        AudioManager.Instance.Play("ItemUse");

        bool hasHUDChanged = false;

        // Healing and Mana regen logic
        if (item.healAmount > 0)
        {
            currentHP = Mathf.Min(currentHP + item.healAmount, maxHP);
            if (unitHUD != null) unitHUD.TriggerStatHighlight("HP");
            Debug.Log($"[{unitName}] Restored {item.healAmount} HP.");
            hasHUDChanged = true;
        }

        if (item.manaRestoreAmount > 0)
        {
            currentMP = Mathf.Min(currentMP + item.manaRestoreAmount, maxMP);
            if (unitHUD != null) unitHUD.TriggerStatHighlight("MP");
            Debug.Log($"[{unitName}] Restored {item.manaRestoreAmount} MP.");
            hasHUDChanged = true;
        }

        // Temporary buff logic
        bool isBuffItem = item.strBonus > 0 || item.defBonus > 0 || item.intBonus > 0 || item.resBonus > 0;

        if (isBuffItem)
        {
            ActiveBuff newBuff = new ActiveBuff
            {
                buffName = item.itemName,
                strAmount = item.strBonus,
                defAmount = item.defBonus,
                intAmount = item.intBonus,
                resAmount = item.resBonus,
                turnsRemaining = item.buffDuration 
            };

            activeBuffs.Add(newBuff);
            RecalculateStats();

            // Trigger visual highlights for stats
            if (unitHUD != null)
            {
                if (item.strBonus > 0) unitHUD.TriggerStatHighlight("STR");
                if (item.defBonus > 0) unitHUD.TriggerStatHighlight("DEF");
                if (item.intBonus > 0) unitHUD.TriggerStatHighlight("INT");
                if (item.resBonus > 0) unitHUD.TriggerStatHighlight("RES");
            }
            
            Debug.Log($"[{unitName}] Applied buff from {item.itemName} for {item.buffDuration} rounds.");
            hasHUDChanged = false;
        }

        if (hasHUDChanged)
        {
            UpdateVisualHUD();
        }
    }

    // Called automatically by BattleManager at the start of this unit's turn
    public void ProcessTurnStart()
    {
        // Reset defend stance
        if (isDefending)
        {
            SetDefend(false);
        }

        // Regen 1 Mana every turn
        if (currentMP < maxMP)
        {
            currentMP++;
            if (unitHUD != null) unitHUD.TriggerStatHighlight("MP");
            UpdateVisualHUD();
        }

        bool statsChanged = false;

        // Reverse loop to remove item buff from list
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            activeBuffs[i].turnsRemaining--;
            if (activeBuffs[i].turnsRemaining <= 0)
            {
                Debug.Log($"[{unitName}] Buff {activeBuffs[i].buffName} has worn off.");
                activeBuffs.RemoveAt(i);
                statsChanged = true;
            }
        }

        // If any buff runs out, recalculate stats
        if (statsChanged)
        {
            RecalculateStats();
        }
    }

    public void ExecuteActionEffects(ActionData action, BattleUnit targetUnit)
    {
        // Consume Health or Mana as cost of the action
        ConsumeCost(action.costType, action.costAmount);

        // Damage calculation
        int baseStat = action.scalingStat == StatScaling.STR ? strength : intelligence;
        int rawDamage = baseStat + action.flatDamageBonus;

        // If player is using Defend, half the damage before calculation
        if (targetUnit.isDefending)
        {
            rawDamage = Mathf.CeilToInt(rawDamage * 0.5f);
        }

        // Substract damage with DEF/RES atribute
        int targetResist = action.scalingStat == StatScaling.STR ? targetUnit.defense : targetUnit.resistance;
        int finalDamage = Mathf.Max(0, rawDamage - targetResist);
        
        // Apply damage to target
        targetUnit.TakeDamage(finalDamage);

        // Process self-restoration (Heal/Mana) from action effects
        if (action.selfHealAmount > 0)
        {
            currentHP = Mathf.Min(currentHP + action.selfHealAmount, maxHP);
            if (unitHUD != null) unitHUD.TriggerStatHighlight("HP");
            Debug.Log($"[{unitName}] Healed {action.selfHealAmount} HP from action effect.");
        }

        if (action.selfManaRestoreAmount > 0)
        {
            currentMP = Mathf.Min(currentMP + action.selfManaRestoreAmount, maxMP);
            if (unitHUD != null) unitHUD.TriggerStatHighlight("MP");
            Debug.Log($"[{unitName}] Restored {action.selfManaRestoreAmount} MP from action effect.");
        }

        // Process temporary attribute buffs from action effects
        bool hasBuff = action.strBuffBonus > 0 || action.defBuffBonus > 0 || action.intBuffBonus > 0 || action.resBuffBonus > 0;
        if (hasBuff)
        {
            ActiveBuff newBuff = new ActiveBuff
            {
                buffName = action.actionName,
                strAmount = action.strBuffBonus,
                defAmount = action.defBuffBonus,
                intAmount = action.intBuffBonus,
                resAmount = action.resBuffBonus,
                turnsRemaining = action.actionBuffDuration 
            };

            activeBuffs.Add(newBuff);
            RecalculateStats(); 

            // Atribute buff HUD highlight
            if (unitHUD != null)
            {
                if (action.strBuffBonus > 0) unitHUD.TriggerStatHighlight("STR");
                if (action.defBuffBonus > 0) unitHUD.TriggerStatHighlight("DEF");
                if (action.intBuffBonus > 0) unitHUD.TriggerStatHighlight("INT");
                if (action.resBuffBonus > 0) unitHUD.TriggerStatHighlight("RES");
            }
        }
        else
        {
            UpdateVisualHUD();
        }
    }

    private void Die()
    {
        Debug.Log($"[{unitName}] Has been defeated!");
    }
}