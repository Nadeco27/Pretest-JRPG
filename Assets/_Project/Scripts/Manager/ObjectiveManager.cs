using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    [Header("System Wireframe")]
    [SerializeField] private ObjectiveUI objectiveUI;
    [SerializeField] private List<ObjectiveData> objectiveQueue = new List<ObjectiveData>();

    private ObjectiveData activeObjective;
    private int currentObjectiveIndex = 0;
    private bool isTransitioning = false;
    private bool isHiddenByDialogue = false;

    private void Awake()
    {
        // Singleton architecture pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        FetchNextObjective();
    }

    // Public global tracker to log mission updates
    public void NotifyObjectiveProgress(string objectiveId)
    {
        // Block updates if no active mission exists, IDs clash, or dialogue mode is on
        if (activeObjective == null || activeObjective.objectiveId != objectiveId || isTransitioning 
            || isHiddenByDialogue) return;

        StartCoroutine(ProcessCompletionSequence());
    }

    // Public method to be invoked via Fungus when dialogue starts
    public void HideUIForDialogue()
    {
        if (activeObjective == null) return;

        isHiddenByDialogue = true;
        objectiveUI.ToggleVisibility(false);
    }

    // Public method to be invoked via Fungus when dialogue concludes
    public void ShowUIAfterDialogue()
    {
        if (activeObjective == null) return;

        isHiddenByDialogue = false;
        objectiveUI.ToggleVisibility(true);
    }

    private void FetchNextObjective()
    {
        if (currentObjectiveIndex >= objectiveQueue.Count)
        {
            activeObjective = null;
            return;
        }

        activeObjective = objectiveQueue[currentObjectiveIndex];

        // Draw objective content only if the view isn't actively blocked by cutscenes
        if (!isHiddenByDialogue)
        {
            objectiveUI.DisplayNewObjective(activeObjective);
        }

        isTransitioning = false;
    }

    private IEnumerator ProcessCompletionSequence()
    {
        isTransitioning = true;
        yield return StartCoroutine(objectiveUI.CompleteObjectiveRoutine(activeObjective));
        currentObjectiveIndex++;
        FetchNextObjective();
    }
}