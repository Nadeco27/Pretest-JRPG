using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class WarningMessageUI : MonoBehaviour
{
    public static WarningMessageUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI warningText;
    
    [Header("Animation Settings")]
    [SerializeField] private float displayDuration = 1.5f;
    [SerializeField] private float fadeDuration = 0.3f;

    private CanvasGroup canvasGroup;
    private Coroutine currentCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
    }

    public void ShowWarning(string message)
    {
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(WarningRoutine(message));
    }

    private IEnumerator WarningRoutine(string message)
    {
        warningText.text = message;
        float timeElapsed = 0f;

        // Fade In
        while (timeElapsed < fadeDuration)
        {
            timeElapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, timeElapsed / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // Hold
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
    }
}