using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Data structure to hold information for each tutorial page
[System.Serializable]
public class TutorialPage
{
    [TextArea(3, 5)]
    [Tooltip("Text explanation for this tutorial page.")]
    public string explanationText;
    
    [Tooltip("Optional image to show. Leave empty if no image needed.")]
    public Sprite optionalImage; 
}

public class BattleTutorialManager : MonoBehaviour
{
    public static BattleTutorialManager Instance { get; private set; }

    [Header("Tutorial Content")]
    [SerializeField] private List<TutorialPage> tutorialPages = new List<TutorialPage>();

    [Header("UI References")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TextMeshProUGUI explanationText;
    [SerializeField] private Image tutorialImage;
    [SerializeField] private TextMeshProUGUI pageIndicatorText;
    
    [Header("Button References")]
    [SerializeField] private Button nextButton;
    [SerializeField] private TextMeshProUGUI nextButtonText;
    [SerializeField] private Button prevButton;

    private int currentPageIndex = 0;
    private bool isTutorialComplete = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Hide panel at start and assign button listeners dynamically
        tutorialPanel.SetActive(false);
        if (nextButton != null) nextButton.onClick.AddListener(OnNextButtonClicked);
        if (prevButton != null) prevButton.onClick.AddListener(OnPrevButtonClicked);
    }

    // Coroutine to show tutorial and pause battle execution until closed
    public IEnumerator ShowTutorialRoutine()
    {
        // Skip if no pages are configured
        if (tutorialPages.Count == 0) yield break;

        isTutorialComplete = false;
        currentPageIndex = 0;
        tutorialPanel.SetActive(true);
        
        UpdatePageUI();

        // Hold the coroutine loop here until the player clicks the Close button
        while (!isTutorialComplete)
        {
            yield return null;
        }

        tutorialPanel.SetActive(false);
    }

    private void UpdatePageUI()
    {
        TutorialPage currentPage = tutorialPages[currentPageIndex];

        explanationText.text = currentPage.explanationText;

        if (currentPage.optionalImage != null)
        {
            tutorialImage.sprite = currentPage.optionalImage;
            tutorialImage.gameObject.SetActive(true);
        }
        else
        {
            tutorialImage.gameObject.SetActive(false);
        }

        pageIndicatorText.text = $"{currentPageIndex + 1}/{tutorialPages.Count}";

        CanvasGroup prevCG = prevButton.GetComponent<CanvasGroup>();
        if (prevCG == null) 
        {
            prevCG = prevButton.gameObject.AddComponent<CanvasGroup>();
        }

        if (currentPageIndex > 0)
        {
            prevButton.interactable = true;
            prevCG.alpha = 1f;
            prevCG.blocksRaycasts = true;
        }
        else
        {
            prevButton.interactable = false;
            prevCG.alpha = 0f;
            prevCG.blocksRaycasts = false;
        }

        if (currentPageIndex == tutorialPages.Count - 1)
        {
            nextButtonText.text = "Close";
        }
        else
        {
            nextButtonText.text = "Next";
        }
    }

    private void OnNextButtonClicked()
    {
        if (currentPageIndex < tutorialPages.Count - 1)
        {
            currentPageIndex++;
            UpdatePageUI();
        }
        else
        {
            // If on the last page, mark as complete to break the coroutine loop
            isTutorialComplete = true; 
        }
    }

    private void OnPrevButtonClicked()
    {
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            UpdatePageUI();
        }
    }
}