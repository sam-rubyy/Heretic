using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerMovement : MonoBehaviour
{
    #region Fields
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private bool useRawInput = true;
    private Vector2 movementInput;
    private Vector2 lastLookDirection = Vector2.right;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        if (body == null)
        {
            body = GetComponent<Rigidbody2D>();
        }
    }

    private void Update()
    {
        if (useRawInput)
        {
            PollRawInput();
        }

        UpdateLookDirection();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }
    #endregion

    #region Public Methods
    public void SetMovementInput(Vector2 input)
    {
        movementInput = input;
    }

    public Vector2 GetLastLookDirection()
    {
        return lastLookDirection;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        SetMovementInput(context.ReadValue<Vector2>());

        if (context.canceled)
        {
            SetMovementInput(Vector2.zero);
        }
    }
    #endregion

    #region Private Methods
    private void PollRawInput()
    {
        Vector2 input = Vector2.zero;

        var gamepad = Gamepad.current;
        if (gamepad != null)
        {
            input = gamepad.leftStick.ReadValue();
        }

        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            float x = 0f;
            float y = 0f;

            if (keyboard.aKey.isPressed) x -= 1f;
            if (keyboard.dKey.isPressed) x += 1f;
            if (keyboard.sKey.isPressed) y -= 1f;
            if (keyboard.wKey.isPressed) y += 1f;

            var keyboardInput = new Vector2(x, y);

            if (keyboardInput.sqrMagnitude > input.sqrMagnitude)
            {
                input = keyboardInput;
            }
        }

        SetMovementInput(input);
    }

    private void HandleMovement()
    {
        var input = movementInput;

        if (input.sqrMagnitude > 1f)
        {
            input = input.normalized;
        }

        var velocity = input * moveSpeed;

        if (body != null)
        {
            if (body.bodyType == RigidbodyType2D.Dynamic)
            {
                body.velocity = velocity;
            }
            else
            {
                var targetPosition = body.position + velocity * Time.fixedDeltaTime;
                body.MovePosition(targetPosition);
            }
            return;
        }

        transform.position += (Vector3)(velocity * Time.fixedDeltaTime);
    }

    private void UpdateLookDirection()
    {
        if (movementInput.sqrMagnitude > 0.001f)
        {
            lastLookDirection = movementInput.normalized;
        }
    }
    #endregion
}
