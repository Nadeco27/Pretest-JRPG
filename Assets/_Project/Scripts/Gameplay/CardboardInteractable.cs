using UnityEngine;

public class CardboardInteractable : InteractableBase
{
    // Override the base Interact method to add specific logic for the cardboard box
    public override void Interact()
    {
        base.Interact();
        
        // Output custom logic
        Debug.Log("Player found something inside the cardboard box!");
    }
}