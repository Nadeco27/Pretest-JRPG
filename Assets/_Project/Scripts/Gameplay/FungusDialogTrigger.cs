using UnityEngine;
using Fungus;

[RequireComponent(typeof(Collider2D))]
public class FungusDialogTrigger : MonoBehaviour
{
    [Header("Persistent Data")]
    [Tooltip("Unique ID for each dialogue trigger")]
    [SerializeField] private string uniqueTriggerID = "Dialog_Default_1";

    [Header("Fungus Settings")]
    [Tooltip("Insert Fungus dialogue flowchart here")]
    [SerializeField] private Flowchart targetFlowchart;

    [Tooltip("Block name in flowchart that will be triggered")]
    [SerializeField] private string targetBlockName;

    [Header("Trigger Settings")]
    [Tooltip("Can dialogue only be triggered once?")]
    [SerializeField] private bool triggerOnlyOnce = true;
    
    [Tooltip("Character tag that can trigger this dialogue")]
    [SerializeField] private string playerTag = "Player";

    private bool hasTriggered = false;

    private void Start()
    {
        // Check if triggerID already triggered
        if (triggerOnlyOnce && PlayerPrefs.GetInt("FungusTrigger_" + uniqueTriggerID, 0) == 1)
        {
            hasTriggered = true;
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(playerTag) && !hasTriggered)
        {
            if (targetFlowchart != null && !string.IsNullOrEmpty(targetBlockName))
            {
                targetFlowchart.ExecuteBlock(targetBlockName);

                if (triggerOnlyOnce)
                {
                    hasTriggered = true;
                    
                    // Save to memory that this dialogue is triggered
                    PlayerPrefs.SetInt("FungusTrigger_" + uniqueTriggerID, 1);
                    PlayerPrefs.Save();

                    Collider2D col = GetComponent<Collider2D>();
                    if (col != null) col.enabled = false;
                }
            }
            else
            {
                Debug.LogWarning("FungusDialogTrigger: Flowchart or Target Block Name not filled on "
                    + gameObject.name);
            }
        }
    }
}