using UnityEngine;
using Fungus;

public class FungusDialogTrigger : MonoBehaviour
{
    [Header("Fungus Settings")]
    [Tooltip("Insert Fungus dialogue flowchart here")]
    [SerializeField] private Flowchart targetFlowchart;

    [Tooltip("Block name in flowchat that will be triggered")]
    [SerializeField] private string targetBlockName;

    [Header("Trigger Settings")]
    [Tooltip("Can dialogue only be triggered once?")]
    [SerializeField] private bool triggerOnlyOnce = true;
    
    [Tooltip("Character tag that can trigger this dialogue")]
    [SerializeField] private string playerTag = "Player";

    private bool hasTriggered = false;

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

                    Collider2D col = GetComponent<Collider2D>();
                    if (col != null) col.enabled = false;
                }
            }
            else
            {
                Debug.LogWarning("FungusDialogTrigger: Flowchart atau Target Block Namenot filled " + 
                    gameObject.name);
            }
        }
    }
}