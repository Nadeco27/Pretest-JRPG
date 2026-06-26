using UnityEngine;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance { get; private set; }

    [Header("System References")]
    [SerializeField] private Transform notificationContainer;
    [SerializeField] private NotificationRowUI notificationPrefab;

    private void Awake()
    {
        // Enforce Singleton architecture
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Spawn new notification row from anywhere in the game
    public void ShowItemNotification(ItemData item, int amount)
    {
        NotificationRowUI newNotification = Instantiate(notificationPrefab, notificationContainer);
        AudioManager.Instance.Play("ItemGet");
        
        // Ensures  newest notification appears at bottom of the list
        newNotification.transform.SetAsLastSibling(); 
        
        newNotification.SetupAndPlay(item, amount);
    }
}