using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 movementInput;

    // Variable to check allowed movement in previous frame
    private bool wasMovementAllowed = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        HandleInput();
        UpdateAnimation();

        // Save movement status in frame to check in next frame
        wasMovementAllowed = PlayerStateManager.isMovementAllowed;
    }

    private void FixedUpdate()
    {
        // Only execute only if player manual movement is allowed
        if (PlayerStateManager.isMovementAllowed)
        {
            MovePlayer();
        }
    }

    private void HandleInput()
    {
        // Block input and reset vector if movement is not allowed
        if (PlayerStateManager.isMovementAllowed == false && InventoryManager.Instance.IsBackpackOpen())
        {
            movementInput = Vector2.zero;
            return;
        }

        // Use GetAxisRaw for movement (-1, 0, or 1)
        movementInput.x = Input.GetAxisRaw("Horizontal");
        movementInput.y = Input.GetAxisRaw("Vertical");

        // Normalize the movement vector to prevent faster diagonal speed
        if (movementInput.sqrMagnitude > 1f)
        {
            movementInput.Normalize();
        }

        // TUTORIAL : If there is any movement input detected, notify the objective manager
        if (movementInput.magnitude > 0f)
        {
            ObjectiveManager.Instance.NotifyObjectiveProgress("WalkTutorial");
        }
    }

    private void UpdateAnimation()
    {
        // Force idle animation if this frame player lose control movement
        if (wasMovementAllowed == true && PlayerStateManager.isMovementAllowed == false)
        {
            animator.SetBool("isWalking", false);
            return;
        }

        // When in cutscene, stop function
        if (PlayerStateManager.isMovementAllowed == false)
        {
            return;
        }

        // Determine if player is actively moving
        bool isWalking = movementInput.magnitude > 0f;
        animator.SetBool("isWalking", isWalking);

        // Only update blend tree directions when moving 
        if (isWalking)
        {
            animator.SetFloat("moveX", movementInput.x);
            animator.SetFloat("moveY", movementInput.y);
        }
    }

    private void MovePlayer()
    {
        // Calculate new position and apply it to the Rigidbody2D
        Vector2 newPosition = rb.position + movementInput * (moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPosition);
    }
}