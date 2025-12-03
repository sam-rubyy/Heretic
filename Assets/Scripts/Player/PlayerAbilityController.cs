using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerAbilityController : MonoBehaviour, IAbilityCollection
{
    #region Fields
    [SerializeField] private List<AbilitySlot> abilities = new List<AbilitySlot>();
    [SerializeField] private MonoBehaviour userOverride;
    [SerializeField] private PlayerAttack playerAttack;
    [SerializeField] private bool clearTargetForPlayerAbilities = true;
    [Header("Input")]
    [SerializeField] private bool useAimedAbilityMode = true;
    [SerializeField] private int aimedAbilitySlotIndex = 2;
    [SerializeField] private bool bindDefaultKeys = true;
    [SerializeField] private List<AbilityBinding> bindings = new List<AbilityBinding>
    {
        new AbilityBinding("Ability 1", 0, Key.Q),
        new AbilityBinding("Ability 2", 1, Key.E),
        new AbilityBinding("Ability 3", 2, Key.R)
    };
    [SerializeField] private List<AbilityIdBinding> abilityIdBindings = new List<AbilityIdBinding>();


    private AbilityContext context;
    private MonoBehaviour owner;
    #endregion

    #region Unity Methods
    private void Reset()
    {
        playerAttack = GetComponent<PlayerAttack>();
        userOverride = this;
    }

    private void Awake()
    {
        if (playerAttack == null)
        {
            playerAttack = GetComponent<PlayerAttack>();
        }

        owner = userOverride != null ? userOverride : (MonoBehaviour)this;
        context = new AbilityContext(owner, null);
        InitializeSlots();
    }

    private void OnEnable()
    {
        SetupBindings();
    }

    private void OnDisable()
    {
        TeardownBindings();
    }
    #endregion

    #region Public Methods
    public AbilitySlot AddAbility(Ability ability, float weightMultiplier = 1f, float cooldownOverride = -1f, bool allowDuplicate = false)
    {
        if (ability == null)
        {
            return null;
        }

        if (!allowDuplicate && abilities.Exists(slot => slot != null && slot.Ability == ability))
        {
            return null;
        }

        var slot = new AbilitySlot(ability, weightMultiplier, cooldownOverride, true);
        slot.Initialize(BulletTimeRunner.GetPlayerTime);
        abilities.Add(slot);
        return slot;
    }

    public bool RemoveAbility(Ability ability, bool onlyRuntimeAdded = true)
    {
        if (ability == null)
        {
            return false;
        }

        for (int i = abilities.Count - 1; i >= 0; i--)
        {
            var slot = abilities[i];
            if (slot == null || slot.Ability != ability)
            {
                continue;
            }

            if (onlyRuntimeAdded && !slot.RuntimeAdded)
            {
                continue;
            }

            abilities.RemoveAt(i);
            return true;
        }

        return false;
    }

    public bool TryUseAbilitySlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= abilities.Count)
        {
            return false;
        }

        return TryUseAbility(abilities[slotIndex], null, GetAimDirection(), clearTargetForPlayerAbilities);
    }

    public bool TryUseAbilityById(string abilityId)
    {
        if (string.IsNullOrWhiteSpace(abilityId))
        {
            return false;
        }

        for (int i = 0; i < abilities.Count; i++)
        {
            var slot = abilities[i];
            if (slot?.Ability != null && slot.Ability.AbilityId == abilityId)
            {
                return TryUseAbilitySlot(i);
            }
        }

        return false;
    }

    public void SetAimDirection(Vector2 direction)
    {
        context?.SetAimDirection(direction);
    }
    #endregion

    #region Private Methods
    private void InitializeSlots()
    {
        for (int i = 0; i < abilities.Count; i++)
        {
            abilities[i]?.Initialize(BulletTimeRunner.GetPlayerTime);
        }
    }

    private Vector2? GetAimDirection()
    {
        if (playerAttack == null)
        {
            return null;
        }

        return playerAttack.GetLastAttackDirection();
    }

    private bool TryUseAbility(AbilitySlot slot, Transform targetOverride, Vector2? aimOverride, bool clearTarget)
    {
        if (slot == null || slot.Ability == null)
        {
            return false;
        }

        if (!slot.IsReady() || !slot.Ability.CanUse(context))
        {
            return false;
        }

        Transform originalTarget = context.Target;
        Vector2 originalAim = context.AimDirection;

        if (clearTarget)
        {
            context.SetTarget(null);
        }
        else if (targetOverride != null)
        {
            context.SetTarget(targetOverride);
        }

        if (aimOverride.HasValue)
        {
            context.SetAimDirection(aimOverride.Value);
        }

        slot.Ability.Activate(context);
        slot.MarkUsed();

        if (clearTarget)
        {
            context.SetTarget(originalTarget);
        }

        if (aimOverride.HasValue)
        {
            context.SetAimDirection(originalAim);
        }

        return true;
    }

    private void SetupBindings()
    {
        if (!bindDefaultKeys || bindings == null)
        {
            return;
        }

        for (int i = 0; i < bindings.Count; i++)
        {
            CreateBindingAction(bindings[i]);
        }

        if (abilityIdBindings != null)
        {
            for (int i = 0; i < abilityIdBindings.Count; i++)
            {
                CreateAbilityIdBindingAction(abilityIdBindings[i]);
            }
        }
    }

    private void TeardownBindings()
    {
        TearDownBindingList(bindings);
        TearDownAbilityIdBindings(abilityIdBindings);
    }

    private void CreateBindingAction(AbilityBinding binding)
    {
        if (binding == null || binding.SlotIndex < 0)
        {
            return;
        }

        string path = GetKeyPath(binding.Key);
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        binding.Action = new InputAction(binding.Label, InputActionType.Button, path);
        binding.Callback = ctx => OnAbilityBinding(ctx, binding.SlotIndex);
        binding.Action.performed += binding.Callback;
        binding.Action.Enable();
    }

    private void CreateAbilityIdBindingAction(AbilityIdBinding binding)
    {
        if (binding == null || string.IsNullOrWhiteSpace(binding.AbilityId))
        {
            return;
        }

        string path = GetKeyPath(binding.Key);
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        binding.Action = new InputAction(binding.Label, InputActionType.Button, path);
        binding.Callback = ctx => OnAbilityIdBinding(ctx, binding.AbilityId);
        binding.Action.performed += binding.Callback;
        binding.Action.Enable();
    }

    private string GetKeyPath(Key key)
    {
        if (key == Key.None)
        {
            return null;
        }

        return $"<Keyboard>/{key.ToString().ToLowerInvariant()}";
    }

    private void OnAbilityBinding(InputAction.CallbackContext context, int slotIndex)
    {
        if (useAimedAbilityMode && slotIndex == aimedAbilitySlotIndex)
        {
            playerAttack?.BeginAbilityAim(slotIndex);
            return;
        }

        TryUseAbilitySlot(slotIndex);
    }

    private void OnAbilityIdBinding(InputAction.CallbackContext context, string abilityId)
    {
        TryUseAbilityById(abilityId);
    }

    private void TearDownBindingList(List<AbilityBinding> list)
    {
        if (list == null)
        {
            return;
        }

        for (int i = 0; i < list.Count; i++)
        {
            var binding = list[i];
            if (binding?.Action == null)
            {
                continue;
            }

            if (binding.Callback != null)
            {
                binding.Action.performed -= binding.Callback;
            }

            binding.Action.Disable();
            binding.Action.Dispose();
            binding.Action = null;
            binding.Callback = null;
        }
    }

    private void TearDownAbilityIdBindings(List<AbilityIdBinding> list)
    {
        if (list == null)
        {
            return;
        }

        for (int i = 0; i < list.Count; i++)
        {
            var binding = list[i];
            if (binding?.Action == null)
            {
                continue;
            }

            if (binding.Callback != null)
            {
                binding.Action.performed -= binding.Callback;
            }

            binding.Action.Disable();
            binding.Action.Dispose();
            binding.Action = null;
            binding.Callback = null;
        }
    }
    #endregion
}

[Serializable]
public class AbilityBinding
{
    #region Fields
    [SerializeField] private string label;
    [SerializeField] private int slotIndex;
    [SerializeField] private Key key;
    #endregion

    #region Properties
    public string Label => label;
    public int SlotIndex => slotIndex;
    public Key Key => key;
    public InputAction Action { get; set; }
    public Action<InputAction.CallbackContext> Callback { get; set; }
    #endregion

    #region Constructors
    public AbilityBinding(string label, int slotIndex, Key key)
    {
        this.label = label;
        this.slotIndex = slotIndex;
        this.key = key;
    }

    public AbilityBinding() : this("Ability", 0, Key.None)
    {
    }
    #endregion
}

[Serializable]
public class AbilityIdBinding
{
    #region Fields
    [SerializeField] private string label;
    [SerializeField] private string abilityId;
    [SerializeField] private Key key;
    #endregion

    #region Properties
    public string Label => label;
    public string AbilityId => abilityId;
    public Key Key => key;
    public InputAction Action { get; set; }
    public Action<InputAction.CallbackContext> Callback { get; set; }
    #endregion

    #region Constructors
    public AbilityIdBinding(string label, string abilityId, Key key)
    {
        this.label = label;
        this.abilityId = abilityId;
        this.key = key;
    }

    public AbilityIdBinding() : this("Ability", "", Key.None)
    {
    }
    #endregion
}
