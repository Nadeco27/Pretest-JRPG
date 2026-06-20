using UnityEngine;
using Fungus;

[RequireComponent(typeof(Collider2D))]
public class NPCInteractable : InteractableBase
{
    [Header("Dialog Settings")]
    [Tooltip("Flowchart Fungus yang berisi naskah dialog NPC ini.")]
    [SerializeField] private Flowchart npcFlowchart;
    
    [Tooltip("Nama Block di dalam Flowchart yang akan dijalankan saat tombol Space ditekan.")]
    [SerializeField] private string dialogueBlockName;

    [Header("Quest Integration")]
    [Tooltip("ID Misi yang akan diselesaikan saat berbicara dengan NPC ini (Kosongkan jika tidak ada).")]
    [SerializeField] private string objectiveToComplete;

    public override void Interact()
    {
        HidePrompt();

        // Complete mission if any
        if (!string.IsNullOrEmpty(objectiveToComplete) && ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.NotifyObjectiveProgress(objectiveToComplete);
        }

        // Fungus dialogue trigger
        if (npcFlowchart != null && npcFlowchart.HasBlock(dialogueBlockName))
        {
            npcFlowchart.ExecuteBlock(dialogueBlockName);
        }
        else
        {
            Debug.LogWarning($"[NPC Interactable] Flowchart atau Block '{dialogueBlockName}' tidak ditemukan pada {gameObject.name}!");
        }
    }
}