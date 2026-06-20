using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private IInteractable currentInteractable;

    private void Update()
    {
        CheckInteractionInput();
    }

    private void CheckInteractionInput()
    {
        // Listen for Space key and trigger interaction if an object is in range
        if (Input.GetKeyDown(KeyCode.Space) && currentInteractable != null)
        {
            if (InventoryManager.Instance != null && InventoryManager.Instance.IsBackpackOpen()) return;

            currentInteractable.Interact();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the collided object implements the IInteractable interface
        IInteractable interactable = collision.GetComponent<IInteractable>();
        if (interactable != null)
        {
            // Turn off previous prompt if the player overlaps multiple objects
            if (currentInteractable != null) currentInteractable.HidePrompt();
            
            currentInteractable = interactable;
            currentInteractable.ShowPrompt();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Clear the reference and hide UI when walking away
        IInteractable interactable = collision.GetComponent<IInteractable>();
        if (interactable != null && interactable == currentInteractable)
        {
            currentInteractable.HidePrompt();
            currentInteractable = null;
        }
    }
}