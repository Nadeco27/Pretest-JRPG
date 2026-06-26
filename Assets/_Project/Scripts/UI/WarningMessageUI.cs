using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class WarningMessageUI : MonoBehaviour
{
    public static WarningMessageUI Instance { get; private set; }

    [Header("UI Component References")]
    [SerializeField] private TextMeshProUGUI warningText;
    [SerializeField] private RectTransform panelRectTransform;

    [Header("Animation Timings")]
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float stayDuration = 1.0f;

    [Header("Shake Customization")]
    [SerializeField] private float shakeMagnitude = 15f;
    [SerializeField] private float shakeSpeed = 50f;

    private CanvasGroup canvasGroup;
    private Vector3 originalLocalPos;
    private Coroutine activeWarningCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        canvasGroup = GetComponent<CanvasGroup>();
        
        // Cache original position to snap back correctly after shaking
        if (panelRectTransform != null) originalLocalPos = panelRectTransform.localPosition;
        else originalLocalPos = transform.localPosition;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void ShowWarning(string message)
    {
        // If a warning is already playing, interrupt it and reset immediately
        if (activeWarningCoroutine != null)
        {
            StopCoroutine(activeWarningCoroutine);
            ResetUIState();
        }

        if (warningText != null)
        {
            warningText.text = message;
        }

        activeWarningCoroutine = StartCoroutine(WarningSequenceRoutine());
    }

    private void ResetUIState()
    {
        canvasGroup.alpha = 0f;
        if (panelRectTransform != null) panelRectTransform.localPosition = originalLocalPos;
        else transform.localPosition = originalLocalPos;
    }

    private IEnumerator WarningSequenceRoutine()
    {
        AudioManager.Instance.Play("FailBuzz");
        Transform targetTransform = panelRectTransform != null ? panelRectTransform : transform;
        float elapsed = 0f;

        // Fade in
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // Shake
        elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float xOffset = Mathf.Sin(Time.time * shakeSpeed) * shakeMagnitude;
            targetTransform.localPosition = new Vector3(originalLocalPos.x + xOffset, originalLocalPos.y, originalLocalPos.z);
            yield return null;
        }
        targetTransform.localPosition = originalLocalPos;

        yield return new WaitForSeconds(stayDuration);

        // Fade out
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        
        activeWarningCoroutine = null;
    }
}