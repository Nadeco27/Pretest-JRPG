using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] private List<ObjectiveGroup> objectiveQueue = new List<ObjectiveGroup>();

    private List<ObjectiveData> currentActiveObjectives = new List<ObjectiveData>();
    private int currentGroupIndex = 0;
    private bool isTransitioning = false;
    private bool isHiddenByDialogue = false;

    // Flag to check backlog objective UI
    private bool pendingUIDisplay = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        FetchNextObjectiveGroup();
    }

    public void NotifyObjectiveProgress(string objectiveId)
    {
        if (isTransitioning || isHiddenByDialogue) return;

        ObjectiveData completedObjective = currentActiveObjectives.Find(o => o.objectiveId == objectiveId);
        
        if (completedObjective != null)
        {
            objectiveUI.CompleteSpecificObjective(objectiveId);
            currentActiveObjectives.Remove(completedObjective);

            ValidateGroupCompletion();
        }
    }

    private void ValidateGroupCompletion()
    {
        bool hasMandatoryLeft = currentActiveObjectives.Exists(o => !o.isOptional);

        if (!hasMandatoryLeft)
        {
            StartCoroutine(ProcessGroupCompletionSequence());
        }
    }

    private IEnumerator ProcessGroupCompletionSequence()
    {
        isTransitioning = true;
        yield return new WaitForSeconds(0.8f); 
        
        objectiveUI.ToggleVisibility(false);
        yield return new WaitForSeconds(0.5f);
        
        currentGroupIndex++;
        FetchNextObjectiveGroup();
    }

    public void SetDialogueState(bool isActive)
    {
        isHiddenByDialogue = isActive;

        // If dialogue done and there is objective UI backlog, then render it
        if (!isActive && pendingUIDisplay)
        {
            objectiveUI.DisplayNewObjectiveGroup(currentActiveObjectives);
            pendingUIDisplay = false;
        }
    }

    private void FetchNextObjectiveGroup()
    {
        if (currentGroupIndex >= objectiveQueue.Count) return;

        ObjectiveGroup nextGroup = objectiveQueue[currentGroupIndex];
        currentActiveObjectives = new List<ObjectiveData>(nextGroup.objectives);

        // If in dialogue, dont render UI, but mark as 'pending'
        if (!isHiddenByDialogue)
        {
            objectiveUI.DisplayNewObjectiveGroup(currentActiveObjectives);
        }
        else
        {
            pendingUIDisplay = true;
        }

        isTransitioning = false;
    }
}