using UnityEngine;
using Unity.Cinemachine;

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager instance;

    [Header("Global Settings")]
    [Tooltip("Kekuatan getaran global yang akan diterapkan ke semua efek shake.")]
    public float globalShakeForce = 1f;

    private void Awake()
    {
        // Setup Singleton
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CameraShake(CinemachineImpulseSource impulseSource)
    {
        impulseSource.GenerateImpulseWithForce(globalShakeForce);
    }
}