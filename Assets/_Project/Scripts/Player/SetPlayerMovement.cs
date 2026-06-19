using UnityEngine;
using Fungus;

[CommandInfo("Player", "Set Movement", "Enable or disable player movement")]
public class SetPlayerMovement : Command
{
    [Tooltip("Check to allow movement, uncheck to disable")]
    public bool isMovementAllowed = false;

    public override void OnEnter()
    {
        PlayerStateManager.isMovementAllowed = isMovementAllowed;
        Continue();
    }
}