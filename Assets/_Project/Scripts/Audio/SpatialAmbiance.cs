using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SpatialAmbiance : MonoBehaviour
{
    private AudioSource ambianceSource;

    private void Start()
    {
        ambianceSource = GetComponent<AudioSource>();

        // Automatically assign the output group to the global SFX group
        if (AudioManager.Instance != null && AudioManager.Instance.sfxGroup != null)
        {
            ambianceSource.outputAudioMixerGroup = AudioManager.Instance.sfxGroup;
        }
        else
        {
            Debug.LogWarning($"[SpatialAmbiance] AudioManager or SFX Group not found on {gameObject.name}");
        }
    }
}