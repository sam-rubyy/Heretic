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
    [Header("Animation")]
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private string fireRateParam = "fireRate";
    [SerializeField] private string shootTrigger = "shoot";
    private Vector2 movementInput;
    private Vector2 lastLookDirection = Vector2.right;
    private Vector2 lastAimDirection = Vector2.right;
    private bool noInput;

    private Vector2 smoothedVelocity;
    private Vector2 smoothVelocityRef;
    private Vector2 knockbackVelocity;
    private float currentSpeed;
    private readonly Collider2D[] overlapBuffer = new Collider2D[12];

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

        if (animator != null)
        {
            //animator.SetBool("noInput", noInput);
            // animator.SetFloat("Blend", movementInput.sqrMagnitude);
            if (!string.IsNullOrEmpty(speedParam))
            {
                animator.SetFloat(speedParam, currentSpeed);
            }
        }

        // flip left / right
        if (spriteRenderer != null)
        {
            if (movementInput.x > 0.01f)
                spriteRenderer.flipX = false;
            else if (movementInput.x < -0.01f)
                spriteRenderer.flipX = true;
            else if (lastAimDirection.x > 0.01f)
                spriteRenderer.flipX = false;
            else if (lastAimDirection.x < -0.01f)
                spriteRenderer.flipX = true;
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

        currentSpeed = finalVelocity.magnitude;
    }

    private bool IsBlocked(Vector2 targetPos)
    {
        // Size of the player's hitbox for checking collisions
        Vector2 boxSize = new Vector2(0.8f, 0.8f);

        int hitCount = Physics2D.OverlapBoxNonAlloc(targetPos, boxSize, 0f, overlapBuffer);
        for (int i = 0; i < hitCount; i++)
        {
            var hit = overlapBuffer[i];
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

    private void UpdateLookDirection()
    {
        if (movementInput.sqrMagnitude > 0.001f)
            lastLookDirection = movementInput.normalized;
    }

    public void SetAimDirection(Vector2 aimDirection)
    {
        if (aimDirection.sqrMagnitude < 0.0001f)
        {
            return;
        }

        lastAimDirection = aimDirection.normalized;
    }

    public void PlayShootAnimation()
    {
        if (animator == null || string.IsNullOrEmpty(shootTrigger))
        {
            return;
        }

        animator.SetTrigger(shootTrigger);
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        if (force <= 0f)
            return;

        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector2.right;

        knockbackVelocity += direction.normalized * force;
    }

    public void SetFireRate(float fireRate)
    {
        if (animator == null || string.IsNullOrEmpty(fireRateParam))
        {
            return;
        }

        animator.SetFloat(fireRateParam, fireRate);
    }
}
