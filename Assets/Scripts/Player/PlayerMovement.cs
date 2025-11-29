using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private bool useRawInput = true;

    private Vector2 movementInput;
    private Vector2 lastLookDirection = Vector2.right;
    private bool noInput;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        // Rigidbody
        if (body == null)
            body = GetComponent<Rigidbody2D>();

        // Animator
        animator = GetComponent<Animator>();

        // Sprite renderer (for left/right flipping)
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // detect if player is moving
        noInput = movementInput == Vector2.zero;

        // update animator parameters
        animator.SetBool("noInput", noInput);
        animator.SetFloat("Blend", movementInput.sqrMagnitude);

        // ⭐ Flip the character left/right
        if (movementInput.x > 0.01f)
            spriteRenderer.flipX = false;  // face right
        else if (movementInput.x < -0.01f)
            spriteRenderer.flipX = true;   // face left

        if (useRawInput)
            PollRawInput();

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

            // pick whichever is stronger (gamepad vs keyboard)
            if (k.sqrMagnitude > input.sqrMagnitude)
                input = k;
        }

        SetMovementInput(input);
    }

    private void HandleMovement()
    {
        var input = movementInput;

        if (input.sqrMagnitude > 1f)
            input = input.normalized;

        Vector2 velocity = input * moveSpeed;

        if (body != null)
        {
            if (body.bodyType == RigidbodyType2D.Dynamic)
                body.velocity = velocity;
            else
                body.MovePosition(body.position + velocity * Time.fixedDeltaTime);

            return;
        }

        transform.position += (Vector3)(velocity * Time.fixedDeltaTime);
    }

    private void UpdateLookDirection()
    {
        if (movementInput.sqrMagnitude > 0.001f)
            lastLookDirection = movementInput.normalized;
    }
}
