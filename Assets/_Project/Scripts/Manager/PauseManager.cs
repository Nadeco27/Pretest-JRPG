using UnityEngine;
using UnityEngine.Audio;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [Header("UI Canvas Wireframe")]
    [SerializeField] private PauseUI pauseUI;
    [SerializeField] private GameObject settingsPanel;

    [Header("Audio Configurations")]
    [SerializeField] private AudioMixer audioMixer;
    [Tooltip("Nama parameter Mixer Exposures wajib sama dengan string ini!")]
    [SerializeField] private string masterVolumeParam = "MasterVol";
    [SerializeField] private string musicVolumeParam = "MusicVol";
    [SerializeField] private string sfxVolumeParam = "SFXVol";

    public bool isPaused { get; private set; } = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Update()
    {
        // Detect Esc button to pause the game
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Do not let player pause the game in the middle of Fungus dialogue
            if (InventoryManager.Instance != null && InventoryManager.Instance.isDialogueActive) return;

            if (isPaused)
            {
                // When settings open, Esc button will close setting, then the pause
                if (settingsPanel != null && settingsPanel.activeSelf)
                {
                    CloseSettings();
                }
                else
                {
                    ResumeGame();
                }
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        
        Time.timeScale = 0f; 
        
        if (pauseUI != null) pauseUI.Show();
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void ResumeGame()
    {
        isPaused = false;
        
        Time.timeScale = 1f;
        AudioManager.Instance.Play("ButtonClick");
        
        if (pauseUI != null) pauseUI.Hide(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
        AudioManager.Instance.Play("ButtonClick");
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        AudioManager.Instance.Play("ButtonClick");
    }

    public void QuitToMainMenu(string mainMenuSceneName)
    {
        Time.timeScale = 1f; 
        isPaused = false;
        AudioManager.Instance.Play("ButtonClick");

        Debug.Log("[PauseManager] Returning to Main Menu...");
        
        if (ScreenTransitionManager.Instance != null)
        {
            ScreenTransitionManager.Instance.SwitchSceneWithTransition(mainMenuSceneName);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    // Autio mixer (will be moved in the future)
    public void SetMasterVolume(float value) => SetMixerVolume(masterVolumeParam, value);
    public void SetMusicVolume(float value) => SetMixerVolume(musicVolumeParam, value);
    public void SetSFXVolume(float value) => SetMixerVolume(sfxVolumeParam, value);

    private void SetMixerVolume(string parameterName, float sliderValue)
    {
        if (audioMixer == null) return;

        float dbValue = Mathf.Log10(Mathf.Max(0.0001f, sliderValue)) * 20f;
        audioMixer.SetFloat(parameterName, dbValue);
    }
}