using UnityEngine;
using Fungus;
using System.Collections;

[CommandInfo("Player", "Set Movement", "Enable or disable player movement")]
public class SetPlayerMovement : Command
{
    [Tooltip("Check to allow movement, uncheck to disable")]
    public bool isMovementAllowed = false;

    public override void OnEnter()
    {
        if (isMovementAllowed)
        {
            StartCoroutine(UnlockMovementDelay());
        }
        else
        {
            PlayerStateManager.isMovementAllowed = false;
            Continue();
        }
    }

    private IEnumerator UnlockMovementDelay()
    {
        // Add input delay to prevent input bleeding
        yield return new WaitForSeconds(0.1f);
        PlayerStateManager.isMovementAllowed = true;
        
        Continue(); 
    }
}