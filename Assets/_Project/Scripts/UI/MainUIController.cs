using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class MainUIController : MonoBehaviour
{
    public static MainUIController Instance { get; private set; }
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // Main core logic for state management
    public void SetDialogueMode(bool isDialogueActive)
    {
        // Smoothly hide or show the entire MainGameCanvas elements
        canvasGroup.alpha = isDialogueActive ? 0f : 1f;
        canvasGroup.interactable = !isDialogueActive;
        canvasGroup.blocksRaycasts = !isDialogueActive;

        // Lock backpack input controls during cutscenes
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.isDialogueActive = isDialogueActive;
        }

        // Freeze objective progression triggers during active dialogue states
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.SetDialogueState(isDialogueActive);
        }
    }

    public void StartDialogueUI()
    {
        SetDialogueMode(true);
    }

    public void EndDialogueUI()
    {
        SetDialogueMode(false);
    }
}