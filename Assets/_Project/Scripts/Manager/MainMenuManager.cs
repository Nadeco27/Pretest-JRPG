using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [Header("Animated Elements")]
    public CanvasGroup[] animatedElements;
    public float animationDuration = 1.2f;
    public float floatOffset = 40f;

    [Header("Menu Panels")]
    public GameObject settingsPanel;

    [Header("Buttons Reference")]
    public Button startButton;
    public Button settingsButton;
    public Button exitButton;
    public Button closeSettingsButton;

    [Header("Volume Sliders Reference")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    private void Start()
    {
        settingsPanel.SetActive(false);

        // Assign button listeners
        startButton.onClick.AddListener(StartGame);
        settingsButton.onClick.AddListener(OpenSettings);
        exitButton.onClick.AddListener(ExitGame);
        closeSettingsButton.onClick.AddListener(CloseSettings);
        InitializeVolumeSliders();

        // Trigger opening animation
        StartCoroutine(AnimateEntrance());
    }

    private void InitializeVolumeSliders()
    {
        // Load saved values from PlayerPrefs
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVol", 0.5f);
            masterVolumeSlider.onValueChanged.AddListener((val) => {
                if (AudioManager.Instance != null) AudioManager.Instance.SetMasterVolume(val);
            });
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVol", 0.5f);
            musicVolumeSlider.onValueChanged.AddListener((val) => {
                if (AudioManager.Instance != null) AudioManager.Instance.SetMusicVolume(val);
            });
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVol", 0.5f);
            sfxVolumeSlider.onValueChanged.AddListener((val) => {
                if (AudioManager.Instance != null) AudioManager.Instance.SetSFXVolume(val);
            });
        }
    }

    private void StartGame()
    {
        // Prevent spam clicking
        startButton.interactable = false;

        ResetProgressionData();
        if (InventoryManager.Instance != null) InventoryManager.Instance.ClearInventory();

        if (ScreenTransitionManager.Instance != null)
        {
            ScreenTransitionManager.Instance.SwitchSceneWithTransition("House Scene");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("House Scene");
        }
    }

    private void ResetProgressionData()
    {
        // Back up audio preferences
        float masterVol = PlayerPrefs.GetFloat("MasterVol", 0.5f);
        float musicVol = PlayerPrefs.GetFloat("MusicVol", 0.5f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVol", 0.5f);

        // Wipe all save data
        PlayerPrefs.DeleteAll();

        // Restore audio preferences
        PlayerPrefs.SetFloat("MasterVol", masterVol);
        PlayerPrefs.SetFloat("MusicVol", musicVol);
        PlayerPrefs.SetFloat("SFXVol", sfxVol);
        PlayerPrefs.Save();

        Debug.Log("[MainMenuManager] Game progression wiped. Audio settings retained.");
    }

    private void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    private void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    private void ExitGame()
    {
        exitButton.interactable = false;

        if (ScreenTransitionManager.Instance != null)
        {
            ScreenTransitionManager.Instance.QuitGameWithTransition();
        }
        else
        {
            Application.Quit();
        }
    }

    private IEnumerator AnimateEntrance()
    {
        Vector2[] targetPositions = new Vector2[animatedElements.Length];
        RectTransform[] rectTransforms = new RectTransform[animatedElements.Length];

        // Capture initial positions and set elements to invisible/lower positions
        for (int i = 0; i < animatedElements.Length; i++)
        {
            animatedElements[i].alpha = 0f;
            rectTransforms[i] = animatedElements[i].GetComponent<RectTransform>();
            targetPositions[i] = rectTransforms[i].anchoredPosition;

            rectTransforms[i].anchoredPosition = new Vector2(targetPositions[i].x, targetPositions[i].y - floatOffset);
        }

        float timer = 0f;

        // Animation loop
        while (timer < animationDuration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / animationDuration);
            float easeProgress = Mathf.SmoothStep(0f, 1f, progress);

            for (int i = 0; i < animatedElements.Length; i++)
            {
                animatedElements[i].alpha = easeProgress;
                rectTransforms[i].anchoredPosition = Vector2.Lerp(
                    new Vector2(targetPositions[i].x, targetPositions[i].y - floatOffset),
                    targetPositions[i],
                    easeProgress
                );
            }

            yield return null;
        }

        // Snap to precise final states
        for (int i = 0; i < animatedElements.Length; i++)
        {
            animatedElements[i].alpha = 1f;
            rectTransforms[i].anchoredPosition = targetPositions[i];
        }
    }
}