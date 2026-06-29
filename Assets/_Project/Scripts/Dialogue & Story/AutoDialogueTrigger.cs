using System.Collections.Generic;
using UnityEngine;
using Fungus;

[System.Serializable]
public class AutoDialogueEvent
{
    [Header("Event Identity")]
    [Tooltip("Event Unique ID")]
    public string uniqueEventID;
    
    [Header("Fungus Wireframe")]
    public Flowchart targetFlowchart;
    public string blockName;

    [Header("Story Requirements (Optional)")]
    [Tooltip("Keep empty if auto execute. Fill if dialogue need story progression first")]
    public string requiredStoryKey;
    [Tooltip("Story key value needed for dialogue to trigger")]
    public int requiredValue = 1;
}

public class AutoDialogueTrigger : MonoBehaviour
{
    [Header("Daftar Dialog Otomatis")]
    [Tooltip("System will check in order, and execute the first fulfilled dialogue")]
    public List<AutoDialogueEvent> dialogEvents = new List<AutoDialogueEvent>();

    private void Start()
    {
        // Check every event in list
        foreach (AutoDialogueEvent evt in dialogEvents)
        {
            // Check if dialogue event already done
            if (PlayerPrefs.GetInt("EventDone_" + evt.uniqueEventID, 0) == 1)
            {
                continue;
            }

            // Check if this event needs story progress requirement
            if (!string.IsNullOrEmpty(evt.requiredStoryKey))
            {
                if (PlayerPrefs.GetInt(evt.requiredStoryKey, 0) < evt.requiredValue)
                {
                    continue; 
                }
            }

            // Execute dialogue that fulfill every check
            PlayerPrefs.SetInt("EventDone_" + evt.uniqueEventID, 1);
            PlayerPrefs.Save();
            
            if (evt.targetFlowchart != null && !string.IsNullOrEmpty(evt.blockName))
            {
                Debug.Log($"[AutoDialogueManager] Trigger dialogue: {evt.blockName} (ID: {evt.uniqueEventID})");
                evt.targetFlowchart.ExecuteBlock(evt.blockName);
            }

            break; 
        }
    }
}