using UnityEngine;

[CreateAssetMenu(fileName = "New Item Data", menuName = "JRPG/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Item Identifiers")]
    public string itemName;
    [TextArea(2, 4)]
    public string itemDescription;
    public Sprite itemIcon;

    [Header("Restoration Effects")]
    [Tooltip("Amount of HP restored when consumed")]
    public int healAmount;
    [Tooltip("Amount of MP restored when consumed")]
    public int manaRestoreAmount;

    [Header("Temporary Stat Buffs")]
    [Tooltip("Amount of Strength added temporarily")]
    public int strBonus;
    
    [Tooltip("Amount of Defense added temporarily")]
    public int defBonus;
    
    [Tooltip("Amount of Intelligence added temporarily")]
    public int intBonus;
    
    [Tooltip("Amount of Resistance added temporarily")]
    public int resBonus;

    [Header("Buff Duration Settings")]
    [Tooltip("How many turn this buff will last?")]
    public int buffDuration = 1;
}