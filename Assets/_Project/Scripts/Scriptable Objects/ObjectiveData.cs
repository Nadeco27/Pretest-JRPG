using UnityEngine;

[CreateAssetMenu(fileName = "NewObjective", menuName = "JRPG/Objective Data")]
public class ObjectiveData : ScriptableObject
{
    public string objectiveId;
    [TextArea(2, 5)] public string description;
    public Sprite emptyCheckboxSprite;
    public Sprite checkedCheckboxSprite;
    
    [Header("Quest Settings")]
    [Tooltip("If true, this objective is not required to progress to the next group.")]
    public bool isOptional;
}