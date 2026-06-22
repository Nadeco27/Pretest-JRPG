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

    [Header("Modular Dialogue Events")]
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
        // Lock the turn-based referee system status
        BattleState previousState = BattleState.PLAYERTURN;
        if (BattleManager.Instance != null)
        {
            previousState = BattleManager.Instance.state;
            BattleManager.Instance.SetBattleState(BattleState.BUSY_CUTSCENE);
        }

        // Execute the specific narrative flowchart block
        if (battleFlowchart.HasBlock(blockName))
        {
            battleFlowchart.ExecuteBlock(blockName);
        }

        yield return null;

        while (battleFlowchart.HasExecutingBlocks())
        {
            yield return null;
        }

        // Resume state balance
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.SetBattleState(previousState);
        }
    }
}