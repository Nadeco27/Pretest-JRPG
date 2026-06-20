using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class NotificationRowUI : MonoBehaviour
{
    [Header("UI Component References")]
    [SerializeField] private TextMeshProUGUI itemText;
    [SerializeField] private Image itemIcon;

    [Header("Animation Timings")]
    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField] private float displayDuration = 2.5f;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
    }

    // Injects the item data and starts the independent fade lifecycle
    public void SetupAndPlay(ItemData item, int amount)
    {
        itemText.text = $"{item.itemName} x{amount}";
        itemIcon.sprite = item.itemIcon;
        
        StartCoroutine(AnimationLifecycleRoutine());
    }

    private IEnumerator AnimationLifecycleRoutine()
    {
        // Fade In
        float timeElapsed = 0f;
        while (timeElapsed < fadeDuration)
        {
            timeElapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, timeElapsed / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // Wait (Read time for the player)
        yield return new WaitForSeconds(displayDuration);

        // Fade Out
        timeElapsed = 0f;
        while (timeElapsed < fadeDuration)
        {
            timeElapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, timeElapsed / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;

        // Self-destruct to keep memory clean
        Destroy(gameObject);
    }
}