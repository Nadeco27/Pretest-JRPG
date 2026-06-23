using System;
using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class UnitAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Curve of running speed of character")]
    public AnimationCurve dashCurve;
    public float dashDuration = 0.3f;

    [Header("Melee Settings")]
    [Tooltip("Stopping distance. If character ran to far, adjust this value")]
    public float meleeOffset = 1.5f;

    public GameObject bloodParticlePrefab;

    private Vector3 startPosition;
    private CinemachineImpulseSource impulseSource;

    private void Awake()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    private void Start()
    {
        startPosition = transform.position;
    }

    public IEnumerator MeleeAttackRoutine(Transform target, Action onHitCallback)
    {
        Vector3 targetPos = target.position;
        
        Vector3 direction = (targetPos - startPosition);
        direction.z = 0f;
        direction = direction.normalized;

        Vector3 attackStancePos = targetPos - (direction * meleeOffset);

        // Run to target
        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            float t = dashCurve.Evaluate(elapsed / dashDuration);
            transform.position = Vector3.Lerp(startPosition, attackStancePos, t);
            yield return null;
        }

        // Hit target
        ExecuteImpactVisuals(target);
        onHitCallback?.Invoke();
        yield return new WaitForSeconds(0.25f);

        // Return to start position
        elapsed = 0f;
        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(attackStancePos, startPosition, elapsed / dashDuration);
            yield return null;
        }
        transform.position = startPosition;
    }

    public IEnumerator RangedAttackRoutine(Transform target, GameObject projectilePrefab, Action onHitCallback)
    {
        // Calculate the recoil target position
        float recoilDirection = startPosition.x < target.position.x ? -0.5f : 0.5f;
        Vector3 recoilPosition = startPosition + new Vector3(recoilDirection, 0, 0);

        float recoilDuration = 0.05f; 
        float recoveryDuration = 0.15f;

        // Animate recoil
        float elapsed = 0f;
        while (elapsed < recoilDuration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPosition, recoilPosition, elapsed / recoilDuration);
            yield return null;
        }
        transform.position = recoilPosition;

        // Fire projectile
        GameObject projObj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        Projectile proj = projObj.GetComponent<Projectile>();
        bool hasHit = false;

        if (proj != null)
        {
            proj.Fire(target, () => 
            {
                ExecuteImpactVisuals(target);
                onHitCallback?.Invoke();
                hasHit = true;
            });
        }

        // Animate recovery
        elapsed = 0f;
        while (elapsed < recoveryDuration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(recoilPosition, startPosition, elapsed / recoveryDuration);
            yield return null;
        }
        transform.position = startPosition;

        yield return new WaitForSeconds(0.2f); 
    }

    private void ExecuteImpactVisuals(Transform targetTransform)
    {
        // Calling screen shake
        if (impulseSource != null && CameraShakeManager.instance != null) 
        {
            CameraShakeManager.instance.CameraShake(impulseSource);
        }
        else if (impulseSource == null)
        {
            Debug.LogWarning("CinemachineImpulseSource tidak ditemukan di GameObject: " + gameObject.name);
        }

        // Memunculkan partikel darah jika ada
        if (bloodParticlePrefab != null) 
        {
            Instantiate(bloodParticlePrefab, targetTransform.position, Quaternion.identity);
        }
    }
}