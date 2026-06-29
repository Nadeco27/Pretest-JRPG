using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fungus;

[System.Serializable]
public class HealthDialogueEvent
{
    [Tooltip("Target enemy HP to trigger this dialogue")]
    public int triggerHealthThreshold;
    
    [Tooltip("Block Flowchart name that will be triggered")]
    public string fungusBlockName;
    
    [HideInInspector] public bool hasTriggered = false;
}

public class BattleCutsceneManager : MonoBehaviour
{
    public static BattleCutsceneManager Instance { get; private set; }

    [Header("Fungus Integration")]
    [SerializeField] private Flowchart battleFlowchart;

    [Header("Dialogue Events")]
    [SerializeField] private List<HealthDialogueEvent> hpDialogueEvents = new List<HealthDialogueEvent>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void CheckEnemyHealthThreshold(int currentEnemyHealth)
    {
        if (battleFlowchart == null) return;

        // Loop through all registered events in the Inspector array
        foreach (HealthDialogueEvent healthEvent in hpDialogueEvents)
        {
            // Check if threshold reached and make sure it has not been executed yet
            if (!healthEvent.hasTriggered && currentEnemyHealth <= healthEvent.triggerHealthThreshold)
            {
                healthEvent.hasTriggered = true;
                StartCoroutine(ExecuteBattleCutscene(healthEvent.fungusBlockName));
                break;
            }
        }
    }

    public IEnumerator ExecuteBattleCutscene(string blockName)
    {
        // Remember the state before the cutscene interruption
        BattleState previousState = BattleState.PLAYERTURN;
        
        if (BattleManager.Instance != null)
        {
            previousState = BattleManager.Instance.state;
            BattleManager.Instance.ChangeState(BattleState.BUSY_CUTSCENE);
        }

        // Execute narrative flowchart block from Fungus
        if (battleFlowchart.HasBlock(blockName))
        {
            battleFlowchart.ExecuteBlock(blockName);
        }

        yield return null;

        while (battleFlowchart.HasExecutingBlocks())
        {
            yield return null;
        }

        if (BattleManager.Instance != null)
        {
            // If there is queue, execute state in queue
            if (BattleManager.Instance.hasPendingState)
            {
                BattleState nextState = BattleManager.Instance.pendingState;
                BattleManager.Instance.hasPendingState = false; // Reset queue
                
                Debug.Log($"[Cutscene] Dialogue finished. Executing state from queue: {nextState}");
                BattleManager.Instance.ChangeState(nextState, true);
            }
            // If no queue, back to previous state
            else
            {
                Debug.Log($"[Cutscene] Dialogue finished. Resuming battle back to: {previousState}");
                BattleManager.Instance.ChangeState(previousState, true);
            }
        }
    }
}