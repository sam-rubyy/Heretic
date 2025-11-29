using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private bool useRawInput = true;

    // Only used for interaction now
    [SerializeField] private LayerMask interactableLayer;   // NPC layer
    [SerializeField] private float interactRadius = 1f;

    private Vector2 movementInput;
    private Vector2 lastLookDirection = Vector2.right;
    private bool noInput;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        if (body == null)
            body = GetComponent<Rigidbody2D>();

        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        noInput = movementInput == Vector2.zero;

        animator.SetBool("noInput", noInput);
        animator.SetFloat("Blend", movementInput.sqrMagnitude);

        // flip left / right
        if (movementInput.x > 0.01f)
            spriteRenderer.flipX = false;
        else if (movementInput.x < -0.01f)
            spriteRenderer.flipX = true;

        if (useRawInput)
            PollRawInput();

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

    private void SetMovementInput(Vector2 input)
    {
        movementInput = input;
    }

    private void PollRawInput()
    {
        Vector2 input = Vector2.zero;

        var gamepad = Gamepad.current;
        if (gamepad != null)
            input = gamepad.leftStick.ReadValue();

        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            float x = 0, y = 0;

            if (keyboard.aKey.isPressed) x -= 1;
            if (keyboard.dKey.isPressed) x += 1;
            if (keyboard.sKey.isPressed) y -= 1;
            if (keyboard.wKey.isPressed) y += 1;

            Vector2 k = new Vector2(x, y);
            if (k.sqrMagnitude > input.sqrMagnitude)
                input = k;
        }

        SetMovementInput(input);
    }

    private void HandleMovement()
    {
        Vector2 input = movementInput;

        if (input.sqrMagnitude > 1f)
            input = input.normalized;

        Vector2 velocity = input * moveSpeed;
        Vector2 targetPos = body.position + velocity * Time.fixedDeltaTime;

        // ⭐ BLOCK ANY SOLID COLLIDER (non-trigger)
        if (IsBlocked(targetPos))
        {
            body.velocity = Vector2.zero;
            return;
        }

        if (body.bodyType == RigidbodyType2D.Dynamic)
            body.velocity = velocity;
        else
            body.MovePosition(targetPos);
    }

    private bool IsBlocked(Vector2 targetPos)
    {
        // Size of the player's hitbox for checking collisions
        Vector2 boxSize = new Vector2(0.8f, 0.8f);

        // Check if we would overlap ANY collider at targetPos
        Collider2D hit = Physics2D.OverlapBox(targetPos, boxSize, 0f);

        // Block only if we hit a non-trigger collider
        return hit != null && !hit.isTrigger;
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

    // Just to see the interaction radius in Scene view (optional)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
