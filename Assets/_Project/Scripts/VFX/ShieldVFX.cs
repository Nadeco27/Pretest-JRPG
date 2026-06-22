using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ShieldVFX : MonoBehaviour
{
    [Header("Shield Animation Settings")]
    [Tooltip("How fast the shield pulses.")]
    [SerializeField] private float pulseSpeed = 3f;
    
    [Tooltip("Minimum transparency of the shield.")]
    [SerializeField] private float minAlpha = 0.2f;
    
    [Tooltip("Maximum transparency of the shield.")]
    [SerializeField] private float maxAlpha = 0.6f;

    private SpriteRenderer shieldRenderer;

    private void Awake()
    {
        shieldRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        // Reset alpha to maximum when the shield is first activated
        if (shieldRenderer != null)
        {
            Color c = shieldRenderer.color;
            c.a = maxAlpha;
            shieldRenderer.color = c;
        }
    }

    private void Update()
    {
        if (shieldRenderer != null)
        {
            float pingPong = Mathf.PingPong(Time.time * pulseSpeed, 1f);
            float currentAlpha = Mathf.Lerp(minAlpha, maxAlpha, pingPong);
            
            // Apply the new alpha to the sprite color
            Color c = shieldRenderer.color;
            c.a = currentAlpha;
            shieldRenderer.color = c;
        }
    }
}