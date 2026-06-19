using UnityEngine;

public class FloatingUI : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float bobSpeed = 5f;
    [SerializeField] private float bobHeight = 0.15f;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.localPosition;
    }

    private void Update()
    {
        // Smooth vertical bobbing effect
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.localPosition = new Vector3(startPosition.x, newY, startPosition.z);
    }
}