using System;
using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float travelSpeed = 15f;
    private Transform targetTransform;
    private Action onImpactCallback;

    public void Fire(Transform target, Action onImpact)
    {
        targetTransform = target;
        onImpactCallback = onImpact;
        StartCoroutine(TravelRoutine());
    }

    private IEnumerator TravelRoutine()
    {
        while (targetTransform != null && Vector3.Distance(transform.position, targetTransform.position) > 0.5f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetTransform.position, travelSpeed * Time.deltaTime);
            yield return null;
        }

        // Trigger damage, shake, and particle when hit
        onImpactCallback?.Invoke();
        Destroy(gameObject);
    }
}