using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerAttack : MonoBehaviour
{
    #region Fields
    [SerializeField] private WeaponBase equippedWeapon;
    private bool attackHeld;
    private Vector2 attackInput;
    private Vector2 lastAttackDirection = Vector2.right;
    private global::InputSystem inputActions;
    private global::InputSystem.PlayerActions playerActions;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        inputActions = new global::InputSystem();
        playerActions = inputActions.Player;
    }

    private void OnEnable()
    {
        playerActions.Enable();
        playerActions.Attack.performed += OnAttackAction;
        playerActions.Attack.canceled += OnAttackAction;
    }

    private void OnDisable()
    {
        playerActions.Attack.performed -= OnAttackAction;
        playerActions.Attack.canceled -= OnAttackAction;
        playerActions.Disable();
    }

    private void OnDestroy()
    {
        playerActions.Disable();
        inputActions.Dispose();
    }

    private void Update()
    {
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
    private void OnAttackAction(InputAction.CallbackContext context)
    {
        var input = context.ReadValue<Vector2>();
        SetAttackInput(input);
        attackHeld = input.sqrMagnitude > 0.01f && context.phase != InputActionPhase.Canceled;
        if (context.canceled)
        {
            SetAttackInput(Vector2.zero);
            attackHeld = false;
        }
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
