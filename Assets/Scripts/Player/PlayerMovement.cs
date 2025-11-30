using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerMovement : MonoBehaviour
    , IKnockbackReceiver
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private float accelerationTime = 0.08f;
    [SerializeField] private float decelerationTime = 0.1f;
    [SerializeField] private float knockbackDamping = 12f;

    // Only used for interaction now
    [SerializeField] private LayerMask interactableLayer;   // NPC layer
    [SerializeField] private float interactRadius = 1f;

    private Vector2 movementInput;
    private Vector2 lastLookDirection = Vector2.right;
    private bool noInput;
    private Vector2 smoothedVelocity;
    private Vector2 smoothVelocityRef;
    private Vector2 knockbackVelocity;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private global::InputSystem inputActions;
    private global::InputSystem.PlayerActions playerActions;

    private void Awake()
    {
        if (body == null)
            body = GetComponent<Rigidbody2D>();

        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        inputActions = new global::InputSystem();
        playerActions = inputActions.Player;
    }

    private void OnEnable()
    {
        playerActions.Enable();
        playerActions.Movement.performed += OnMovementAction;
        playerActions.Movement.canceled += OnMovementAction;
    }

    private void OnDisable()
    {
        playerActions.Movement.performed -= OnMovementAction;
        playerActions.Movement.canceled -= OnMovementAction;
        playerActions.Disable();
    }

    private void OnDestroy()
    {
        playerActions.Disable();
        inputActions.Dispose();
    }

    private void Update()
    {
        noInput = movementInput == Vector2.zero;

        animator.SetBool("noInput", noInput);
        // animator.SetFloat("Blend", movementInput.sqrMagnitude);

        // flip left / right
        if (movementInput.x > 0.01f)
            spriteRenderer.flipX = false;
        else if (movementInput.x < -0.01f)
            spriteRenderer.flipX = true;

        // Press E to interact with NPC
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryInteract();
        }

        UpdateLookDirection();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        SetMovementInput(context.ReadValue<Vector2>());

        if (context.canceled)
            SetMovementInput(Vector2.zero);
    }

    private void OnMovementAction(InputAction.CallbackContext context)
    {
        OnMove(context);
    }

    private void SetMovementInput(Vector2 input)
    {
        movementInput = input;
    }

    private void HandleMovement()
    {
        Vector2 input = movementInput;

        if (input.sqrMagnitude > 1f)
            input = input.normalized;

        float smoothTime = input.sqrMagnitude > 0.001f ? accelerationTime : decelerationTime;
        smoothedVelocity = Vector2.SmoothDamp(smoothedVelocity, input * moveSpeed, ref smoothVelocityRef, smoothTime);

        // Apply knockback decay
        knockbackVelocity = Vector2.MoveTowards(knockbackVelocity, Vector2.zero, knockbackDamping * Time.fixedDeltaTime);

        Vector2 moveDelta = (smoothedVelocity + knockbackVelocity) * Time.fixedDeltaTime;
        Vector2 nextPos = body.position;

        if (Mathf.Abs(moveDelta.x) > Mathf.Epsilon)
        {
            Vector2 attempt = nextPos + new Vector2(moveDelta.x, 0f);
            if (!IsBlocked(attempt))
                nextPos = attempt;
            else
            {
                smoothedVelocity.x = 0f;
                smoothVelocityRef.x = 0f;
            }
        }

        if (Mathf.Abs(moveDelta.y) > Mathf.Epsilon)
        {
            Vector2 attempt = nextPos + new Vector2(0f, moveDelta.y);
            if (!IsBlocked(attempt))
                nextPos = attempt;
            else
            {
                smoothedVelocity.y = 0f;
                smoothVelocityRef.y = 0f;
            }
        }

        Vector2 finalVelocity = (nextPos - body.position) / Time.fixedDeltaTime;

        if (body.bodyType == RigidbodyType2D.Dynamic)
            body.velocity = finalVelocity;
        else
            body.MovePosition(nextPos);
    }

    private bool IsBlocked(Vector2 targetPos)
    {
        // Size of the player's hitbox for checking collisions
        Vector2 boxSize = new Vector2(0.8f, 0.8f);

        Collider2D[] hits = Physics2D.OverlapBoxAll(targetPos, boxSize, 0f);

        for (int i = 0; i < hits.Length; i++)
        {
            var hit = hits[i];
            if (hit == null || hit.isTrigger)
                continue;

            // Ignore ourselves and other characters so we don't get glued when overlapping.
            if (hit.attachedRigidbody == body)
                continue;
            if (hit.CompareTag("Player"))
                continue;
            if (hit.GetComponent<EnemyBase>() != null || hit.CompareTag("Enemy"))
                continue;

            return true;
        }

        return false;
    }

    private void TryInteract()
    {
        // Check for NPCs in a circle around the player
        Collider2D hit = Physics2D.OverlapCircle(
            transform.position,
            interactRadius,
            interactableLayer
        );

        if (hit != null)
        {
            Debug.Log("NPC detected: " + hit.name);
        }
        else
        {
            Debug.Log("No NPC nearby.");
        }
    }

    private void UpdateLookDirection()
    {
        if (movementInput.sqrMagnitude > 0.001f)
            lastLookDirection = movementInput.normalized;
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        if (force <= 0f)
            return;

        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector2.right;

        knockbackVelocity += direction.normalized * force;
    }

    // Just to see the interaction radius in Scene view (optional)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
