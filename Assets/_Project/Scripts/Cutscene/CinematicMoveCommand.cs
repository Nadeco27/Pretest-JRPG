using UnityEngine;
using Fungus;
using System;

[CommandInfo("Player", "Cinematic Move", "Move player to a target position cinematically")]
public class CinematicMoveCommand : Command
{
    public CinematicMovement playerCinematic;
    public Transform targetDestination;

    public override void OnEnter()
    {
        if (playerCinematic != null && targetDestination != null)
        {
            Action onComplete = FinishCinematic;
            playerCinematic.MoveToTarget(targetDestination, onComplete);
        }
        else
        {
            Continue();
        }
    }

    private void FinishCinematic()
    {
        Continue();
    }
}