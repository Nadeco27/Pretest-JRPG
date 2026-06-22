using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "JRPG/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    [TextArea(2, 4)] public string itemDescription;
    public Sprite itemIcon;

    [Header("Battle Effects")]
    [Tooltip("Amount of HP to restore.")]
    public int healHP;
    
    [Tooltip("Amount of MP to restore.")]
    public int restoreMP;

    [Header("Attribute Buffs")]
    public int buffSTR;
    public int buffDEF;
    public int buffINT;
    public int buffRES;
}