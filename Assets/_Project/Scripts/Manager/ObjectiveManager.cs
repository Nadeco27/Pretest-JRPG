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
            // Save cleared objective status to player prefs
            PlayerPrefs.SetInt("ObjCompleted_" + objectiveId, 1);
            PlayerPrefs.Save();
            
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
        isTransitioning = false; 
        
        FetchNextObjectiveGroup();
    }

    public void SetDialogueState(bool isActive)
    {
        isHiddenByDialogue = isActive;

        // If dialogue done and there is objective UI backlog, then render it
        if (!isActive && pendingUIDisplay)
        {
            // Make sure there is available objectiveto complete before showing UI
            if (currentActiveObjectives.Count > 0)
            {
                objectiveUI.DisplayNewObjectiveGroup(currentActiveObjectives);
            }
            pendingUIDisplay = false;
        }
    }

    private void FetchNextObjectiveGroup()
    {
        if (currentGroupIndex >= objectiveQueue.Count) return;

        ObjectiveGroup nextGroup = objectiveQueue[currentGroupIndex];
        
        // Filter showing new uncleared objective 
        currentActiveObjectives = new List<ObjectiveData>();
        
        // Put uncleared objective to active objective list
        foreach (ObjectiveData obj in nextGroup.objectives)
        {
            if (PlayerPrefs.GetInt("ObjCompleted_" + obj.objectiveId, 0) == 0)
            {
                currentActiveObjectives.Add(obj);
            }
        }

        // Check if objective group cleared, and then skip it
        bool hasMandatoryLeft = currentActiveObjectives.Exists(o => !o.isOptional);
        if (!hasMandatoryLeft && nextGroup.objectives.Count > 0)
        {
            currentGroupIndex++;
            FetchNextObjectiveGroup();
            return;
        }

        if (!isHiddenByDialogue)
        {
            objectiveUI.DisplayNewObjectiveGroup(currentActiveObjectives);
        }
        else
        {
            pendingUIDisplay = true;
        }
    }
}