using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class InventoryItem
{
    public ItemData data;
    public int quantity;

    public InventoryItem(ItemData item, int amt)
    {
        data = item;
        quantity = amt;
    }
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("UI Bridge Link")]
    [SerializeField] private BackpackUI backpackUI;
    public bool isDialogueActive = false;

    // Master database data collection list
    private List<InventoryItem> inventoryDatabase = new List<InventoryItem>();

    private void Awake()
    {
        // Enforce persistent singleton architecture
        if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
    }

    private void Update()
    {
        HandleKeyboardInput();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isDialogueActive = false;
    }

    // Dynamic UI binding registration method
    public void RegisterBackpackUI(BackpackUI ui)
    {
        backpackUI = ui;
    }

    public void ToggleBackpackFromButton()
    {
        if (isDialogueActive) return;
        backpackUI.ToggleBackpack(inventoryDatabase);
    }

    private void HandleKeyboardInput()
    {
        if (isDialogueActive) return;

        // Toggle bag display via "B" key code
        if (Input.GetKeyDown(KeyCode.B))
        {
            backpackUI.ToggleBackpack(inventoryDatabase);
        }

        // Close bag layout via "Escape" key code if active
        if (Input.GetKeyDown(KeyCode.Escape) && backpackUI.IsOpen())
        {
            backpackUI.HideBackpack();
        }
    }

    // Public method called by the box trigger logic or drop tables
    public void AddItemToInventory(ItemData item, int amount)
    {
        // Find if the item category already exists inside the structural list array
        InventoryItem existingItem = inventoryDatabase.Find(i => i.data == item);

        if (existingItem != null)
        {
            // If it exists, remove it first to push it to the front of the list later
            inventoryDatabase.Remove(existingItem);
            existingItem.quantity += amount;
            
            // Insert at index 0 so newest item goes to the leftmost box
            inventoryDatabase.Insert(0, existingItem);
        }
        else
        {
            // Brand new item entry tracking registration
            inventoryDatabase.Insert(0, new InventoryItem(item, amount));
        }

        // If the backpack screen is currently active, redraw changes immediately
        if (backpackUI.IsOpen())
        {
            backpackUI.RefreshInventoryUI(inventoryDatabase);
        }
    }

    // Helper conversion method to pipe raw database data arrays directly
    public List<InventoryItem> GetInventoryList() => inventoryDatabase;
    public bool IsBackpackOpen() => backpackUI != null && backpackUI.IsOpen();
}