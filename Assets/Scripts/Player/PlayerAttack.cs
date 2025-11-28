using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerAttack : MonoBehaviour
{
    #region Fields
    [SerializeField] private WeaponBase equippedWeapon;
    [SerializeField] private bool useRawInput = true;
    private bool attackHeld;
    private Vector2 attackInput;
    private Vector2 lastAttackDirection = Vector2.right;
    #endregion

    #region Unity Methods
    private void Update()
    {
        if (useRawInput)
        {
            PollRawInput();
        }

        HandleAttackInput();
    }
    #endregion

    #region Public Methods
    public void SetWeapon(WeaponBase weapon)
    {
        equippedWeapon = weapon;
    }

    public void SetAttackInput(Vector2 input)
    {
        attackInput = input;

        if (attackInput.sqrMagnitude > 0.001f)
        {
            lastAttackDirection = attackInput.normalized;
        }
    }

    public Vector2 GetLastAttackDirection()
    {
        return lastAttackDirection;
    }

    public void TryAttack()
    {
        if (equippedWeapon == null)
        {
            return;
        }

        if (!equippedWeapon.CanFire())
        {
            return;
        }

        equippedWeapon.HandleAttack(lastAttackDirection);
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        var input = context.ReadValue<Vector2>();
        SetAttackInput(input);
        attackHeld = input.sqrMagnitude > 0.01f;

        if (context.canceled)
        {
            SetAttackInput(Vector2.zero);
            attackHeld = false;
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
            input = gamepad.rightStick.ReadValue();
        }

        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            float x = 0f;
            float y = 0f;

            if (keyboard.leftArrowKey.isPressed) x -= 1f;
            if (keyboard.rightArrowKey.isPressed) x += 1f;
            if (keyboard.downArrowKey.isPressed) y -= 1f;
            if (keyboard.upArrowKey.isPressed) y += 1f;

            var keyboardInput = new Vector2(x, y);

            if (keyboardInput.sqrMagnitude > input.sqrMagnitude)
            {
                input = keyboardInput;
            }
        }

        SetAttackInput(input);
        attackHeld = input.sqrMagnitude > 0.01f;
    }

    private void HandleAttackInput()
    {
        if (!attackHeld)
        {
            return;
        }

        TryAttack();
    }
    #endregion
}
