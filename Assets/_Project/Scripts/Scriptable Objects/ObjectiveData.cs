using UnityEngine;

[CreateAssetMenu(fileName = "NewObjective", menuName = "JRPG/Objective Data")]
public class ObjectiveData : ScriptableObject
{
    // Unique identifier to match logic triggers in the game
    public string objectiveId;

    // The text that will guide the player
    [TextArea(2, 5)] public string description;

    // Sprite assets for the checkbox states
    public Sprite emptyCheckboxSprite;
    public Sprite checkedCheckboxSprite;
}