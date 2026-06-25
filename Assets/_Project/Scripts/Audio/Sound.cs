using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    [Header("Audio Identity")]
    public string id; // The string ID to call this sound
    public AudioClip clip;

    [Header("Audio Settings")]
    [Range(0f, 1f)] public float volume = 1f;
    [Range(.1f, 3f)] public float pitch = 1f;
    public bool loop;

    [Header("Random Pitch Setup")]
    [Tooltip("If true, the pitch will slightly random every time it plays")]
    public bool randomizePitch = false;
    [Range(0f, 1f)] public float pitchRandomness = 0.1f;

    [Header("Mixer Routing")]
    [Tooltip("Check this if this is BGM. Uncheck if this is SFX")]
    public bool isMusic = false;

    [HideInInspector] public AudioSource source;
}