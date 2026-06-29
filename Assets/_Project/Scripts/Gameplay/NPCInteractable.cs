using UnityEngine;
using Fungus;

[RequireComponent(typeof(Collider2D))]
public class NPCInteractable : InteractableBase
{
    [Header("Dialog Settings")]
    [SerializeField] private Flowchart npcFlowchart;
    [SerializeField] private string dialogueBlockName;

    [Header("Quest Integration")]
    [Tooltip("Completed mission ID. Can be triggered automatically or via fungus")]
    [SerializeField] private string objectiveToComplete;

    public override void Interact()
    {
        // If dialogue is still running, then ignore input
        if (npcFlowchart != null && npcFlowchart.HasExecutingBlocks()) return;

        HidePrompt();

        // Fungus dialogue trigger
        if (npcFlowchart != null && npcFlowchart.HasBlock(dialogueBlockName))
        {
            npcFlowchart.ExecuteBlock(dialogueBlockName);
        }
        else
        {
            Debug.LogWarning($"[NPC Interactable] Flowchart or Block '{dialogueBlockName}' not found!");
        }
    }

    public void CompleteAssociatedObjective()
    {
        if (!string.IsNullOrEmpty(objectiveToComplete) && ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.NotifyObjectiveProgress(objectiveToComplete);
            Debug.Log($"[NPC Interactable] Quest '{objectiveToComplete}' completed succesfully");
        }
    }
}