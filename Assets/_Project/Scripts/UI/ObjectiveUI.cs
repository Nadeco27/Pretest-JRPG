using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class ObjectiveUI : MonoBehaviour
{
    [Header("UI Component References")]
    [SerializeField] private Image statusImage;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Fade Animation Adjustments")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float completeDisplayDelay = 0.8f;

    private CanvasGroup canvasGroup;
    private Coroutine activeFadeRoutine;

    private void Awake()
    {
        // Initialize and ensure UI starts fully transparent
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
    }

    // Handles drawing a brand new objective to the HUD layout
    public void DisplayNewObjective(ObjectiveData data)
    {
        statusImage.sprite = data.emptyCheckboxSprite;
        descriptionText.text = data.description;
        TriggerFade(1f);
    }

    // Handles the checked box transition before dissolving away completely
    public IEnumerator CompleteObjectiveRoutine(ObjectiveData data)
    {
        statusImage.sprite = data.checkedCheckboxSprite;
        yield return new WaitForSeconds(completeDisplayDelay);
        yield return StartCoroutine(FadeCanvasRoutine(0f));
    }

    // Public controller method to safely trigger interpolation toggle
    public void ToggleVisibility(bool isVisible)
    {
        TriggerFade(isVisible ? 1f : 0f);
    }

    // Directs the routine pipeline and avoids animation overlap conflicts
    private void TriggerFade(float targetAlpha)
    {
        if (activeFadeRoutine != null)
        {
            StopCoroutine(activeFadeRoutine);
        }
        activeFadeRoutine = StartCoroutine(FadeCanvasRoutine(targetAlpha));
    }

    // Coroutine calculation to interpolate transparency alpha over time
    private IEnumerator FadeCanvasRoutine(float targetAlpha)
    {
        float startAlpha = canvasGroup.alpha;
        float timeElapsed = 0f;

        while (timeElapsed < fadeDuration)
        {
            timeElapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timeElapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        activeFadeRoutine = null;
    }
}