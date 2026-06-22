using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleHUD : MonoBehaviour
{
    [Header("Health Bar Components")]
    [SerializeField] private Image hpFillImage;
    [SerializeField] private TextMeshProUGUI hpText;

    [Header("Mana Bar Components")]
    [SerializeField] private Image mpFillImage;
    [SerializeField] private TextMeshProUGUI mpText;

    [Header("Attribute UI Text Fields")]
    [SerializeField] private TextMeshProUGUI strText;
    [SerializeField] private TextMeshProUGUI defText;
    [SerializeField] private TextMeshProUGUI intText;
    [SerializeField] private TextMeshProUGUI resText;

    // Core method to handle HP and MP fill bars
    public void UpdateHUD(int currentHP, int maxHP, int currentMP, int maxMP)
    {
        if (hpFillImage != null)
        {
            hpFillImage.fillAmount = (float)currentHP / maxHP;
        }
        if (hpText != null)
        {
            hpText.text = $"{currentHP}/{maxHP}";
        }

        if (mpFillImage != null)
        {
            mpFillImage.fillAmount = (float)currentMP / maxMP;
        }
        if (mpText != null)
        {
            mpText.text = $"{currentMP}/{maxMP}";
        }
    }

    public void UpdateAttributes(int str, int def, int intel, int res)
    {
        if (strText != null) strText.text = str.ToString();
        if (defText != null) defText.text = def.ToString();
        if (intText != null) intText.text = intel.ToString();
        if (resText != null) resText.text = res.ToString();
    }

    public void TriggerStatHighlight(string statName)
    {
        TextMeshProUGUI targetText = null;

        // Map the string to the correct UI component
        switch (statName)
        {
            case "HP": targetText = hpText; break;
            case "MP": targetText = mpText; break;
            case "STR": targetText = strText; break;
            case "DEF": targetText = defText; break;
            case "INT": targetText = intText; break;
            case "RES": targetText = resText; break;
        }

        if (targetText != null)
        {
            StartCoroutine(HighlightTextRoutine(targetText));
        }
    }

    private System.Collections.IEnumerator HighlightTextRoutine(TextMeshProUGUI textUI)
    {
        Color originalColor = Color.white;
        Vector3 originalScale = Vector3.one;
        Vector3 punchScale = Vector3.one * 1.5f;
        Color buffColor = Color.green;

        float duration = 0.4f;
        float elapsed = 0f;

        // Pop out and turn green immediately
        textUI.color = buffColor;
        textUI.transform.localScale = punchScale;

        // Smoothly return to normal size and color
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            textUI.color = Color.Lerp(buffColor, originalColor, t);
            textUI.transform.localScale = Vector3.Lerp(punchScale, originalScale, t);
            
            yield return null;
        }

        textUI.color = originalColor;
        textUI.transform.localScale = originalScale;
    }
}