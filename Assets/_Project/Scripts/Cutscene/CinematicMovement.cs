using UnityEngine;
using System.Collections;
using System;

public class CinematicMovement : MonoBehaviour
{
    private float moveSpeed = 3.0f;
    private Rigidbody2D rb;
    private Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    public void MoveToTarget(Transform targetTransform, Action onComplete)
    {
        StartCoroutine(MoveRoutine(targetTransform.position, onComplete));
    }

    private IEnumerator MoveRoutine(Vector2 targetPosition, Action onComplete)
    {
        animator.SetBool("isWalking", true);

        while (Vector2.Distance(rb.position, targetPosition) > 0.1f)
        {
            // Calculate move direction and start moving to target position
            Vector2 moveDirection = (targetPosition - rb.position).normalized;
            Vector2 newPosition = rb.position + moveDirection * (moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPosition);

            animator.SetFloat("moveX", moveDirection.x);
            animator.SetFloat("moveY", moveDirection.y);

            yield return new WaitForFixedUpdate();
        }

        animator.SetBool("isWalking", false);
        onComplete?.Invoke();
    }
}