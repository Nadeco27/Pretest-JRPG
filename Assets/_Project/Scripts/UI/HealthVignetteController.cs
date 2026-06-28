using UnityEngine;
using UnityEngine.UI;

public class HealthVignetteController : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private Image vignetteImage;
    
    [Header("Threshold Settings")]
    [Tooltip("Health treshold (persentage) for health vignette to show up")]
    [SerializeField] private float activationThreshold = 0.4f;

    public void UpdateVignette(float currentHP, float maxHP)
    {
        if (vignetteImage == null || vignetteImage.material == null) return;

        // Calculate health persentage
        float hpPercentage = currentHP / maxHP;

        float targetIntensity = 0f;

        if (hpPercentage <= activationThreshold)
        {
            targetIntensity = 1f - (hpPercentage / activationThreshold);
        }

        vignetteImage.material.SetFloat("_Intensity", targetIntensity);
    }

    private void OnDisable()
    {
        // Reset vignette when player reset battle (vignette image disabled)
        if (vignetteImage != null && vignetteImage.material != null)
        {
            vignetteImage.material.SetFloat("_Intensity", 0f);
        }
    }
}