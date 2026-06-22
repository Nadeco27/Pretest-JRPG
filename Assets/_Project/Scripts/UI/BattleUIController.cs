using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class BattleUIController : MonoBehaviour
{
    public static BattleUIController Instance { get; private set; }

    [Header("Fade Configurations")]
    [SerializeField] private float fadeDuration = 0.5f;

    private CanvasGroup canvasGroup;
    private Coroutine currentFadeCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void HideBattleUI()
    {
        SafeFade(0f, false);
    }

    public void ShowBattleUI()
    {
        SafeFade(1f, true);
    }

    private void SafeFade(float targetAlpha, bool interactable)
    {
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }
        currentFadeCoroutine = StartCoroutine(FadeRoutine(targetAlpha, interactable));
    }

    private IEnumerator FadeRoutine(float targetAlpha, bool interactable)
    {
        float startAlpha = canvasGroup.alpha;
        float timeElapsed = 0f;

        // Block elements from registering clicks during fade
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        while (timeElapsed < fadeDuration)
        {
            timeElapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timeElapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        canvasGroup.interactable = interactable;
        canvasGroup.blocksRaycasts = interactable;
    }
}