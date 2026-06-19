using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Wrapper class to allow configuring lists of lists in the Unity Inspector
[System.Serializable]
public class ObjectiveGroup
{
    public List<ObjectiveData> objectives;
}

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    [Header("System Wireframe")]
    [SerializeField] private ObjectiveUI objectiveUI;
    [Tooltip("Each element represents a phase in the game, containing multiple objectives.")]
    [SerializeField] private List<ObjectiveGroup> objectiveQueue = new List<ObjectiveGroup>();

    private List<ObjectiveData> currentActiveObjectives = new List<ObjectiveData>();
    private int currentGroupIndex = 0;
    private bool isTransitioning = false;
    private bool isHiddenByDialogue = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        FetchNextObjectiveGroup();
    }

    // Triggers completion for a specific objective ID
    public void NotifyObjectiveProgress(string objectiveId)
    {
        if (isTransitioning || isHiddenByDialogue) return;

        // Search if the reported objective is currently active
        ObjectiveData completedObjective = currentActiveObjectives.Find(o => o.objectiveId == objectiveId);
        
        if (completedObjective != null)
        {
            objectiveUI.CompleteSpecificObjective(objectiveId);
            currentActiveObjectives.Remove(completedObjective); // Remove from tracking list

            ValidateGroupCompletion();
        }
    }

    // Checks if we should move to the next phase based on mandatory requirements
    private void ValidateGroupCompletion()
    {
        // Check if there are any mandatory (non-optional) objectives left
        bool hasMandatoryLeft = currentActiveObjectives.Exists(o => !o.isOptional);

        if (!hasMandatoryLeft)
        {
            StartCoroutine(ProcessGroupCompletionSequence());
        }
    }

    private IEnumerator ProcessGroupCompletionSequence()
    {
        isTransitioning = true;
        
        // Brief pause to let player see the final checkbox tick
        yield return new WaitForSeconds(0.8f); 
        
        objectiveUI.ToggleVisibility(false); // Fade out the whole panel
        yield return new WaitForSeconds(0.5f);
        
        currentGroupIndex++;
        FetchNextObjectiveGroup();
    }

    public void HideUIForDialogue()
    {
        isHiddenByDialogue = true;
        objectiveUI.ToggleVisibility(false);
    }

    public void ShowUIAfterDialogue()
    {
        isHiddenByDialogue = false;
        objectiveUI.ToggleVisibility(true);
    }

    private void FetchNextObjectiveGroup()
    {
        if (currentGroupIndex >= objectiveQueue.Count) return;

        // Copy the objectives from the master queue to the active tracking list
        ObjectiveGroup nextGroup = objectiveQueue[currentGroupIndex];
        currentActiveObjectives = new List<ObjectiveData>(nextGroup.objectives);

        if (!isHiddenByDialogue)
        {
            objectiveUI.DisplayNewObjectiveGroup(currentActiveObjectives);
        }

        isTransitioning = false;
    }
}