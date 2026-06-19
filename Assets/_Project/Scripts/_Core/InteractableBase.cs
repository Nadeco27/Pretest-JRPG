using UnityEngine;

public abstract class InteractableBase : MonoBehaviour, IInteractable
{
    [Header("UI Feedback")]
    [Tooltip("Reference to the floating spacebar prompt object above the item")]
    [SerializeField] private GameObject interactPrompt;

    private void Awake()
    {
        // Ensure the prompt is hidden at the start of the game
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }
    }

    public virtual void Interact()
    {
        // Base interaction logic, meant to be overridden by child classes
        Debug.Log("Interacting with: " + gameObject.name);

        ObjectiveManager.Instance.NotifyObjectiveProgress("InteractTutorial");
    }

    public void ShowPrompt()
    {
        if (interactPrompt != null) interactPrompt.SetActive(true);
    }

    public void HidePrompt()
    {
        if (interactPrompt != null) interactPrompt.SetActive(false);
    }
}