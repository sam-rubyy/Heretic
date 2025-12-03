using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerAttack : MonoBehaviour
{
    #region Fields
    [SerializeField] private WeaponBase equippedWeapon;
    [SerializeField] private PlayerAbilityController abilityController;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private bool allowAutoFire = true;
    private bool attackHeld;
    private Vector2 lastAttackDirection = Vector2.right;
    private global::InputSystem inputActions;
    private global::InputSystem.PlayerActions playerActions;
    private InputAction fireAction;
    private bool abilityAiming;
    private int abilitySlotToUse = -1;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        inputActions = new global::InputSystem();
        playerActions = inputActions.Player;
        if (abilityController == null)
        {
            abilityController = GetComponent<PlayerAbilityController>();
        }
        abilityController?.SetAimDirection(lastAttackDirection);

        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
        }

        fireAction = new InputAction("Fire", InputActionType.Button, "<Keyboard>/space");
    }

    private void OnEnable()
    {
        playerActions.Enable();
        playerActions.Attack.performed += OnAttackAction;
        playerActions.Attack.canceled += OnAttackAction;

        fireAction.Enable();
        fireAction.performed += OnFireAction;
    }

    private void OnDisable()
    {
        playerActions.Attack.performed -= OnAttackAction;
        playerActions.Attack.canceled -= OnAttackAction;
        playerActions.Disable();

        fireAction.performed -= OnFireAction;
        fireAction.Disable();
    }

    private void OnDestroy()
    {
        playerActions.Disable();
        inputActions.Dispose();

        fireAction?.Dispose();
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
        if (input.sqrMagnitude > 0.001f)
        {
            lastAttackDirection = input.normalized;
        }
        abilityController?.SetAimDirection(lastAttackDirection);
        playerMovement?.SetAimDirection(lastAttackDirection);
    }

    public Vector2 GetLastAttackDirection()
    {
        return lastAttackDirection;
    }

    public void BeginAbilityAim(int slotIndex)
    {
        abilityAiming = true;
        abilitySlotToUse = slotIndex;
        attackHeld = false; // stop weapon autofire while aiming abilities
    }

    public void CancelAbilityAim()
    {
        abilityAiming = false;
        abilitySlotToUse = -1;
    }

    public void TryAttack()
    {
        if (abilityAiming)
        {
            return;
        }

        if (equippedWeapon == null)
        {
            return;
        }

        if (!equippedWeapon.CanFire())
        {
            return;
        }

        playerMovement?.SetFireRate(equippedWeapon.GetCurrentFireRateForAnimation());
        equippedWeapon.HandleAttack(lastAttackDirection);
        playerMovement?.PlayShootAnimation();
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        ProcessAttackContext(context);
    }

    private void OnFireAction(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            if (abilityAiming)
            {
                if (abilitySlotToUse >= 0)
                {
                    abilityController?.TryUseAbilitySlot(abilitySlotToUse);
                }

                CancelAbilityAim();
            }
            else
            {
                TryAttack();
            }
        }
    }
    #endregion

    #region Private Methods
    private void OnAttackAction(InputAction.CallbackContext context)
    {
        ProcessAttackContext(context);
    }

    private void HandleAttackInput()
    {
        if (!allowAutoFire || abilityAiming)
        {
            return;
        }

        if (attackHeld)
        {
            TryAttack();
        }
    }

    private void ProcessAttackContext(InputAction.CallbackContext context)
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
    #endregion
}
