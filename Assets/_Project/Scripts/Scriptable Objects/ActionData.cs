using UnityEngine;

// Pindahkan enum ini ke sini agar bersifat global
public enum ActionCostType { None, HP, MP }
public enum ActionAnimationType { Melee, Ranged }
public enum StatScaling { STR, INT }

[CreateAssetMenu(fileName = "New Action", menuName = "JRPG/Action Data")]
public class ActionData : ScriptableObject
{
    [Header("General Info")]
    public string actionName;
    [TextArea(2, 4)] public string description;
    
    [Header("Cost Requirements")]
    public ActionCostType costType;
    public int costAmount;

    [Header("Execution Settings")]
    public ActionAnimationType animationType;
    [Tooltip("Don't fill if melee, fill if ranged")]
    public GameObject projectilePrefab;

    [Header("Combat Effects")]
    public StatScaling scalingStat = StatScaling.STR;
    [Tooltip("Bonus flat damage (ex : fill 1 for STR+1)")]
    public int flatDamageBonus = 0;
    [Tooltip("Health regenerated when using this action")]
    public int selfHealAmount = 0;
    [Tooltip("Mana rengenerated when using this action")]
    public int selfManaRestoreAmount = 0;

    [Header("Temporary Buff Effects")]
    public int strBuffBonus;
    public int defBuffBonus;
    public int intBuffBonus;
    public int resBuffBonus;

    [Tooltip("How many turn this buff will last?")]
    public int actionBuffDuration = 1;
}