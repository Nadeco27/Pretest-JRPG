using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DynamicTooltip : MonoBehaviour
{
    public static DynamicTooltip Instance { get; private set; }

    [Header("UI Component References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    
    [Header("Layout Reference")]
    [SerializeField] private RectTransform rectTransform;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        HideTooltip();
    }

    private void Update()
    {
        // Continuously track mouse position if tooltip is visible
        if (gameObject.activeSelf)
        {
            UpdatePosition();
        }
    }

    public void ShowTooltip(string title, string cost, string description)
    {
        gameObject.SetActive(true);
        
        titleText.text = title;
        costText.text = cost;
        descriptionText.text = description;
        
        // Force Unity layout engine to recalculate size immediately based on text length
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        
        UpdatePosition();
    }

    public void HideTooltip()
    {
        gameObject.SetActive(false);
    }

    private void UpdatePosition()
    {
        Vector2 mousePos = Input.mousePosition;
        
        // Normalize mouse coordinates
        float normalizedX = mousePos.x / Screen.width;
        float normalizedY = mousePos.y / Screen.height;

        // Smart pivot calculation so tooltip can appear in right or left side of cursor
        float pivotX = normalizedX > 0.5f ? 1.05f : -0.05f;
        float pivotY = normalizedY > 0.5f ? 1.05f : -0.05f;

        rectTransform.pivot = new Vector2(pivotX, pivotY);
        rectTransform.position = mousePos;
    }
}