using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Fungus;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Mixer Reference")]
    public AudioMixer mainMixer;
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup sfxGroup;

    // Add fungus mixer to control of dialogue text volume
    [Header("Fungus Integration")]
    public AudioMixer fungusMixer; 
    [SerializeField] private string fungusVolumeParam = "Volume";

    [Header("Sound Collection")]
    public List<Sound> sounds;

    private Dictionary<string, Sound> soundDictionary;
    private Sound _currentMusic;

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

        InitializeSounds();
    }
    
    private void Start()
    {
        // Load saved volume settings from PlayerPrefs, default to 50% if not found
        float masterVol = PlayerPrefs.GetFloat("MasterVol", 0.5f);
        float musicVol = PlayerPrefs.GetFloat("MusicVol", 0.5f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVol", 0.5f);

        SetMasterVolume(masterVol);
        SetMusicVolume(musicVol);
        SetSFXVolume(sfxVol);

        StartCoroutine(HijackFungusAudio());
        // Check which scene is active to play BGM
        TriggerSceneBGM(SceneManager.GetActiveScene().name);
    }

    private void InitializeSounds()
    {
        soundDictionary = new Dictionary<string, Sound>();

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            
            s.source.outputAudioMixerGroup = s.isMusic ? musicGroup : sfxGroup;

            soundDictionary[s.id] = s;
        }
    }

    
    // Play SFX with random pitch modifier
    public void Play(string id)
    {
        if (soundDictionary.TryGetValue(id, out Sound s))
        {
            if (s.randomizePitch)
            {
                s.source.pitch = s.pitch + Random.Range(-s.pitchRandomness, s.pitchRandomness);
            }
            else
            {
                // Reset to original pitch
                s.source.pitch = s.pitch;
            }
            
            s.source.Play();
        }
        else
        {
            Debug.LogWarning($"[AudioManager] Sound ID: '{id}' not found!");
        }
    }

    // Play BGM with smooth crossfade
    public void PlayMusic(string id, float fadeDuration = 1f)
    {
        if (soundDictionary.TryGetValue(id, out Sound newMusic))
        {
            // Prevent restarting if the same music is already playing
            if (_currentMusic == newMusic && newMusic.source.isPlaying) return;

            StartCoroutine(FadeMusicRoutine(newMusic, fadeDuration));
        }
        else
        {
            Debug.LogWarning($"[AudioManager] Music ID: '{id}' not found!");
        }
    }

    private IEnumerator FadeMusicRoutine(Sound newMusic, float duration)
    {
        Sound oldMusic = _currentMusic;
        _currentMusic = newMusic;

        newMusic.source.volume = 0f;
        newMusic.source.Play();

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime; 
            float progress = Mathf.Clamp01(timer / duration);

            newMusic.source.volume = Mathf.Lerp(0f, newMusic.volume, progress);

            if (oldMusic != null && oldMusic.source != null)
            {
                oldMusic.source.volume = Mathf.Lerp(oldMusic.volume, 0f, progress);
            }

            yield return null;
        }

        newMusic.source.volume = newMusic.volume;
        
        if (oldMusic != null && oldMusic.source != null)
        {
            oldMusic.source.volume = 0f;
            oldMusic.source.Stop(); 
            oldMusic.source.volume = oldMusic.volume;
        }
    }

    private IEnumerator HijackFungusAudio()
    {
        yield return null; 

        // Search for Fungus MusicManager in scene
        MusicManager fungusMusic = FindFirstObjectByType<MusicManager>();
        
        // If none, force Fungus to show the MusicManager
        if (fungusMusic == null && FungusManager.Instance != null)
        {
            fungusMusic = FungusManager.Instance.GetComponent<MusicManager>();
        }

        if (fungusMusic != null)
        {
            // Take over all of AudioSource
            AudioSource[] fungusSources = fungusMusic.GetComponents<AudioSource>();
            
            if (fungusSources.Length >= 5)
            {
                // Audio source 0 : Background Music
                fungusSources[0].outputAudioMixerGroup = musicGroup;
                
                // Audio source 1 to 4: Ambiance,  Sound Effect, Voice, and blip audio
                for (int i = 1; i <= 4; i++)
                {
                    fungusSources[i].outputAudioMixerGroup = sfxGroup;
                }
                
                Debug.Log("[AudioManager] Sucesfully take over Fungus AudioManager and send it to AudioMixer");
            }
        }
    }

    private void TriggerSceneBGM(string sceneName)
    {
        Debug.Log($"[AudioManager] Moving to scene: '{sceneName}'. Checking music.");

        if (sceneName == "Main Menu" || sceneName == "House Scene")
        {
            PlayMusic("MainMusic", 1.5f);
        }
        else if (sceneName == "Dream Scene")
        {
            PlayMusic("DreamMusic", 1.5f);
        }
        else if (sceneName == "Battle Scene")
        {
            PlayMusic("BattleMusic", 0.5f); 
        }
    }

    public void UpdateMusicForScene(string sceneName)
    {
        Debug.Log($"[AudioManager] Scene '{sceneName}' loaded, adjusting music..");
        TriggerSceneBGM(sceneName);
    }

    // Audio mixer integration
    public void SetMasterVolume(float sliderValue)
    {
        PlayerPrefs.SetFloat("MasterVol", sliderValue);
        mainMixer.SetFloat("MasterVol", Mathf.Log10(Mathf.Max(0.0001f, sliderValue)) * 20f);
    }
    
    public void SetMusicVolume(float sliderValue)
    {
        PlayerPrefs.SetFloat("MusicVol", sliderValue);
        mainMixer.SetFloat("MusicVol", Mathf.Log10(Mathf.Max(0.0001f, sliderValue)) * 20f);
    }
    
    public void SetSFXVolume(float sliderValue)
    {
        // Save slider value
        PlayerPrefs.SetFloat("SFXVol", sliderValue);
        
        float dbValue = Mathf.Log10(Mathf.Max(0.0001f, sliderValue)) * 20f;
        
        // Set value to both main mixer and fungus mixer
        mainMixer.SetFloat("SFXVol", dbValue);
        if (fungusMixer != null)
        {
            fungusMixer.SetFloat(fungusVolumeParam, dbValue);
        }
    }
}