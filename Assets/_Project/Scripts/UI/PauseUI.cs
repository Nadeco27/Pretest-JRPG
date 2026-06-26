using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PauseUI : MonoBehaviour
{
    [Header("UI Container References")]
    [SerializeField] private GameObject container;
    [SerializeField] private RectTransform visualContent;
    [SerializeField] private CanvasGroup backgroundOverlay;
    
    [Header("Pause Menu Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitToMainMenuButton;

    [Header("Settings Panel Sliders")]
    [SerializeField] private Button closeSettingsButton;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Scene Config")]
    [SerializeField] private string mainMenuSceneName = "Main Menu";

    [Header("CRT Animation Settings")]
    [SerializeField] private float animationDuration = 0.25f;

    private void Start()
    {
        // Setup Buttons
        resumeButton.onClick.AddListener(() => PauseManager.Instance.ResumeGame());
        settingsButton.onClick.AddListener(() => PauseManager.Instance.OpenSettings());
        
        quitToMainMenuButton.onClick.AddListener(() => {
            Hide(true); 
            PauseManager.Instance.QuitToMainMenu(mainMenuSceneName);
        });

        closeSettingsButton.onClick.AddListener(() => PauseManager.Instance.CloseSettings());

        // Connect all volume slider to AudioManger
        masterVolumeSlider.onValueChanged.AddListener((val) => AudioManager.Instance.SetMasterVolume(val));
        musicVolumeSlider.onValueChanged.AddListener((val) => AudioManager.Instance.SetMusicVolume(val));
        sfxVolumeSlider.onValueChanged.AddListener((val) => AudioManager.Instance.SetSFXVolume(val));

        // Load saved values from PlayerPrefs
        masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVol", 0.5f);
        musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVol", 0.5f);
        sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVol", 0.5f);

        container.SetActive(false);
    }

    public void Show()
    {
        container.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(AnimateCRT_Open());
    }

    public void Hide(bool instant = false)
    {
        if (instant)
        {
            container.SetActive(false);
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(AnimateCRT_Close());
        }
    }

    // CRT open and close pause animation
    private IEnumerator AnimateCRT_Open()
    {
        float timer = 0f;
        visualContent.localScale = new Vector3(1f, 0.005f, 1f);
        backgroundOverlay.alpha = 0f;

        // Fade in background overlay
        while (timer < animationDuration / 2)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / (animationDuration / 2);
            backgroundOverlay.alpha = Mathf.Lerp(0f, 0.5f, t);
            yield return null;
        }

        // Stretch vertically
        timer = 0f;
        while (timer < animationDuration / 2)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / (animationDuration / 2);
            visualContent.localScale = new Vector3(1f, Mathf.Lerp(0.005f, 1f, t), 1f);
            backgroundOverlay.alpha = Mathf.Lerp(0.5f, 1f, t);
            yield return null;
        }

        visualContent.localScale = Vector3.one;
        backgroundOverlay.alpha = 1f;
    }

    private IEnumerator AnimateCRT_Close()
    {
        float timer = 0f;

        while (timer < animationDuration / 2)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / (animationDuration / 2);
            visualContent.localScale = new Vector3(1f, Mathf.Lerp(1f, 0.005f, t), 1f);
            backgroundOverlay.alpha = Mathf.Lerp(1f, 0.5f, t);
            yield return null;
        }

        timer = 0f;
        while (timer < animationDuration / 2)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / (animationDuration / 2);
            backgroundOverlay.alpha = Mathf.Lerp(0.5f, 0f, t);
            yield return null;
        }

        backgroundOverlay.alpha = 0f;
        container.SetActive(false);
    }
}