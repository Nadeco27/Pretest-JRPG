using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "JRPG/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public string itemDescription;
    public Sprite itemIcon;
}