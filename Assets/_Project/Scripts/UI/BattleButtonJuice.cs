using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class BattleButtonJuice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Juice Scale Settings")]
    [SerializeField] private float hoverScaleAmount = 1.08f;
    [SerializeField] private float animationDuration = 0.12f;

    [Header("Ghost Overlay Integration")]
    [Tooltip("Assign aura GameObject behind the main action menu button")]
    [SerializeField] private CanvasGroup hoverOverlay;
    [SerializeField] private float maxOverlayAlpha = 0.6f;

    private Vector3 originalScale;
    private Button targetButton;

    private void Awake()
    {
        targetButton = GetComponent<Button>();
        originalScale = transform.localScale;
        
        if (hoverOverlay != null)
        {
            hoverOverlay.alpha = 0f;
            hoverOverlay.transform.localScale = Vector3.one;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (targetButton != null && !targetButton.interactable) return;

        transform.DOKill();
        if (hoverOverlay != null) hoverOverlay.DOKill();

        // Uniform scale up 
        transform.DOScale(originalScale * hoverScaleAmount, animationDuration).SetEase(Ease.OutBack);
        
        // Pulse Aura
        if (hoverOverlay != null)
        {
            hoverOverlay.DOFade(maxOverlayAlpha, animationDuration).SetEase(Ease.OutQuad);
            hoverOverlay.transform.DOScale(Vector3.one * 1.15f, animationDuration).SetEase(Ease.OutCubic);
        }
        
        if (AudioManager.Instance != null) AudioManager.Instance.Play("ButtonHover");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOKill();
        if (hoverOverlay != null) hoverOverlay.DOKill();

        // Reset elements back to standard state properties
        transform.DOScale(originalScale, animationDuration).SetEase(Ease.OutQuad);
        
        if (hoverOverlay != null)
        {
            hoverOverlay.DOFade(0f, animationDuration).SetEase(Ease.OutQuad);
            hoverOverlay.transform.DOScale(Vector3.one, animationDuration).SetEase(Ease.OutQuad);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (targetButton != null && !targetButton.interactable) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            transform.DOKill();
            transform.DOPunchScale(new Vector3(0.12f, 0.12f, 0f), 0.1f, 10, 1f);

            AudioManager.Instance.Play("ButtonClick");
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (BattleActionMenu.Instance != null)
            {
                BattleActionMenu.Instance.CloseCurrentSubMenu();
                BattleActionMenu.Instance.TriggerCancelJuice();

                AudioManager.Instance.Play("ButtonClick");
            }
        }
    }

    private void OnDisable()
    {
        transform.DOKill();
        if (hoverOverlay != null) hoverOverlay.DOKill();
        
        transform.localScale = originalScale;
        if (hoverOverlay != null)
        {
            hoverOverlay.alpha = 0f;
            hoverOverlay.transform.localScale = Vector3.one;
        }
    }
}