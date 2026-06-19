using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class ObjectiveUI : MonoBehaviour
{
    [Header("UI Spawning References")]
    [SerializeField] private Transform rowsContainer;
    [SerializeField] private ObjectiveRowUI rowPrefab;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.5f;

    private CanvasGroup canvasGroup;
    private Coroutine activeFadeRoutine;
    
    // Tracks spawned rows by their objective ID for easy access
    private Dictionary<string, ObjectiveRowUI> activeRows = new Dictionary<string, ObjectiveRowUI>();

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
    }

    // Instantiates UI prefabs for every objective in the current group
    public void DisplayNewObjectiveGroup(List<ObjectiveData> objectives)
    {
        // Clean up previous objective rows
        foreach (Transform child in rowsContainer) 
        {
            Destroy(child.gameObject);
        }
        activeRows.Clear();

        // Spawn and map new rows
        foreach (var obj in objectives)
        {
            ObjectiveRowUI newRow = Instantiate(rowPrefab, rowsContainer);
            newRow.Initialize(obj);
            activeRows.Add(obj.objectiveId, newRow);
        }

        TriggerFade(1f);
    }

    // Targets a specific row to update its checkbox
    public void CompleteSpecificObjective(string objectiveId)
    {
        if (activeRows.TryGetValue(objectiveId, out ObjectiveRowUI row))
        {
            row.MarkComplete();
        }
    }

    public void ToggleVisibility(bool isVisible)
    {
        TriggerFade(isVisible ? 1f : 0f);
    }

    private void TriggerFade(float targetAlpha)
    {
        if (activeFadeRoutine != null) StopCoroutine(activeFadeRoutine);
        activeFadeRoutine = StartCoroutine(FadeCanvasRoutine(targetAlpha));
    }

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