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

    private Canvas parentCanvas;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        parentCanvas = GetComponentInParent<Canvas>();

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

        // Smart Pivot Calculation
        float pivotX = normalizedX > 0.5f ? 1.05f : -0.05f;
        float pivotY = normalizedY > 0.5f ? 1.05f : -0.05f;

        rectTransform.pivot = new Vector2(pivotX, pivotY);

        if (parentCanvas != null && parentCanvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            // Convert pixel screen position to local canvas position using the render camera
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform, 
                mousePos, 
                parentCanvas.worldCamera, 
                out Vector2 localPoint);

            rectTransform.localPosition = localPoint;
        }
        else
        {
            rectTransform.position = mousePos;
        }
    }
}