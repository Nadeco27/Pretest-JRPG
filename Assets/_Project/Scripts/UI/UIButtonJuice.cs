using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UIButtonJuice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Juice Settings")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float clickScale = 0.9f;
    [SerializeField] private float animationDuration = 0.25f;

    [Header("Sway Settings")]
    [Tooltip("Maximum rotation angle for the right-left sway effect.")]
    [SerializeField] private float swayAmount = 10f;
    [Tooltip("How much the button wobbles back and forth inside the duration.")]
    [SerializeField] private int swayVibrato = 5;

    private Vector3 originalScale;
    private Vector3 originalRotation;

    private void Awake()
    {
        originalScale = transform.localScale;
        
        // If button recorded size 0, force to be 1
        if (originalScale == Vector3.zero) originalScale = Vector3.one;

        originalRotation = transform.localEulerAngles;
    }

    private void Start()
    {
        originalScale = transform.localScale;
        originalRotation = transform.localEulerAngles;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(originalScale * hoverScale, animationDuration).SetEase(Ease.OutBack).SetUpdate(true);
        transform.DOPunchRotation(new Vector3(0, 0, swayAmount), animationDuration * 2.5f, swayVibrato, 0.5f).SetUpdate(true);
        
        AudioManager.Instance.Play("ButtonHover");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(originalScale, animationDuration).SetEase(Ease.OutQuad).SetUpdate(true);
        transform.DOLocalRotate(originalRotation, animationDuration).SetEase(Ease.OutQuad).SetUpdate(true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(originalScale * clickScale, animationDuration / 2f).SetEase(Ease.OutQuad).SetUpdate(true);
        
        AudioManager.Instance.Play("ButtonClick");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(originalScale * hoverScale, animationDuration / 2f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    private void OnDisable()
    {
        transform.DOKill();
        transform.localScale = originalScale;
        transform.localEulerAngles = originalRotation;
    }
}