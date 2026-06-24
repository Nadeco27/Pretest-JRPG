using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class UnitVisualController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Coroutine to fade out sprite
    public IEnumerator FadeOutRoutine(float duration)
    {
        if (spriteRenderer == null) yield break;

        float elapsedTime = 0f;
        Color startColor = spriteRenderer.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            
            float newAlpha = Mathf.Lerp(startColor.a, 0f, elapsedTime / duration);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, newAlpha);
            
            yield return null;
        }

        spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
        gameObject.SetActive(false); 
    }
}