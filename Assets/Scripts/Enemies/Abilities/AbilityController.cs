using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class AbilityController : MonoBehaviour, IAbilityCollection
{
    #region Fields
    [SerializeField] private List<AbilitySlot> abilities = new List<AbilitySlot>();
    [SerializeField] private Transform target;
    [SerializeField] private MonoBehaviour userOverride;
    [SerializeField] private float decisionInterval = 0.25f;
    [SerializeField] private bool autoUseAbilities = true;
    [SerializeField] private bool autoFindPlayerTarget = true;
    private float lastDecisionTime;
    private AbilityContext context;
    private MonoBehaviour owner;
    #endregion

    #region Properties
    public IReadOnlyList<AbilitySlot> AbilitySlots => abilities;
    public bool AutoUseAbilities
    {
        get => autoUseAbilities;
        set => autoUseAbilities = value;
    }
    #endregion

    #region Unity Methods
    private void Awake()
    {
        owner = userOverride;
        if (owner == null)
        {
            owner = GetComponent<EnemyBase>();
        }
        if (owner == null)
        {
            owner = GetComponentInParent<PlayerAttack>();
        }
        if (owner == null)
        {
            owner = GetComponentInParent<PlayerMovement>();
        }
        if (owner == null)
        {
            owner = this;
        }
        context = new AbilityContext(owner, target);
        InitializeSlots();
        EnsureTarget();
    }

    private void Update()
    {
        if (!autoUseAbilities)
        {
            return;
        }

        if (Time.time < lastDecisionTime + decisionInterval)
        {
            return;
        }

        lastDecisionTime = Time.time;
        TryUseRandomAbility();
    }
    #endregion

    #region Public Methods
    public AbilityContext GetContext()
    {
        return EnsureContext();
    }

    public void SetTarget(Transform targetTransform)
    {
        target = targetTransform;
        EnsureContext().SetTarget(target);
    }

    public void SetAimDirection(Vector2 direction)
    {
        EnsureContext().SetAimDirection(direction);
    }

    public void SetOwner(MonoBehaviour newOwner)
    {
        if (newOwner == null)
        {
            return;
        }

        owner = newOwner;
        context = new AbilityContext(owner, target);
    }

    public void ResetCooldowns()
    {
        InitializeSlots();
    }

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
        slot.Initialize();
        abilities.Add(slot);
        return slot;
    }

    public bool TryUseAbilityAtIndex(int index, Transform targetOverride = null, Vector2? aimOverride = null, bool clearTarget = false)
    {
        if (index < 0 || index >= abilities.Count)
        {
            return false;
        }

        var slot = abilities[index];
        return TryUseAbility(slot, targetOverride, aimOverride, clearTarget);
    }

    public bool TryUseAbility(Ability ability, Transform targetOverride = null, Vector2? aimOverride = null, bool clearTarget = false)
    {
        if (ability == null)
        {
            return false;
        }

        for (int i = 0; i < abilities.Count; i++)
        {
            if (abilities[i]?.Ability == ability)
            {
                return TryUseAbilityAtIndex(i, targetOverride, aimOverride, clearTarget);
            }
        }

        return false;
    }

    public bool TryUseAbility(string abilityId, Transform targetOverride = null, Vector2? aimOverride = null, bool clearTarget = false)
    {
        if (string.IsNullOrWhiteSpace(abilityId))
        {
            return false;
        }

        for (int i = 0; i < abilities.Count; i++)
        {
            var slot = abilities[i];
            if (slot?.Ability == null)
            {
                continue;
            }

            if (slot.Ability.AbilityId == abilityId)
            {
                return TryUseAbilityAtIndex(i, targetOverride, aimOverride, clearTarget);
            }
        }

        return false;
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

    public bool TryUseRandomAbility()
    {
        var readySlots = GetReadySlots();
        if (readySlots.Count == 0)
        {
            return false;
        }

        var chosen = ChooseWeightedSlot(readySlots);
        if (chosen == null)
        {
            return false;
        }

        return TryUseAbility(chosen, null, null, false);
    }
    #endregion

    #region Private Methods
    private AbilityContext EnsureContext()
    {
        if (context == null)
        {
            owner = owner != null ? owner : (MonoBehaviour)this;
            context = new AbilityContext(owner, target);
        }

        return context;
    }

    private void InitializeSlots()
    {
        for (int i = 0; i < abilities.Count; i++)
        {
            abilities[i]?.Initialize();
        }
    }

    private void EnsureTarget()
    {
        if (!autoFindPlayerTarget || target != null)
        {
            return;
        }

        if (owner != null && owner.CompareTag("Player"))
        {
            return;
        }

        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            SetTarget(player.transform);
        }
    }

    private List<AbilitySlot> GetReadySlots()
    {
        var ready = new List<AbilitySlot>();
        for (int i = 0; i < abilities.Count; i++)
        {
            var slot = abilities[i];
            if (slot == null || slot.Ability == null)
            {
                continue;
            }

            if (!slot.IsReady())
            {
                continue;
            }

            if (!slot.Ability.CanUse(EnsureContext()))
            {
                continue;
            }

            ready.Add(slot);
        }

        return ready;
    }

    private AbilitySlot ChooseWeightedSlot(List<AbilitySlot> slots)
    {
        if (slots == null || slots.Count == 0)
        {
            return null;
        }

        float totalWeight = 0f;
        for (int i = 0; i < slots.Count; i++)
        {
            totalWeight += slots[i].Weight;
        }

        if (totalWeight <= 0.0001f)
        {
            return slots[0];
        }

        float roll = UnityEngine.Random.value * totalWeight;
        for (int i = 0; i < slots.Count; i++)
        {
            float weight = slots[i].Weight;
            if (weight <= 0f)
            {
                continue;
            }

            roll -= weight;
            if (roll <= 0f)
            {
                return slots[i];
            }
        }

        return slots[slots.Count - 1];
    }

    private bool TryUseAbility(AbilitySlot slot, Transform targetOverride, Vector2? aimOverride, bool clearTarget)
    {
        if (slot == null || slot.Ability == null)
        {
            return false;
        }

        var ctx = EnsureContext();
        if (!slot.IsReady() || !slot.Ability.CanUse(ctx))
        {
            return false;
        }

        Transform originalTarget = ctx.Target;
        Vector2 originalAim = ctx.AimDirection;

        bool overrideTarget = clearTarget || targetOverride != null;
        if (overrideTarget)
        {
            ctx.SetTarget(targetOverride);
        }

        if (aimOverride.HasValue)
        {
            ctx.SetAimDirection(aimOverride.Value);
        }

        slot.Ability.Activate(ctx);
        slot.MarkUsed();

        if (overrideTarget)
        {
            ctx.SetTarget(originalTarget);
        }

        if (aimOverride.HasValue)
        {
            ctx.SetAimDirection(originalAim);
        }

        return true;
    }
    #endregion
}

[System.Serializable]
public class AbilitySlot
{
    #region Fields
    [SerializeField] private Ability ability;
    [SerializeField] private float weightMultiplier = 1f;
    [SerializeField] private float cooldownOverride = -1f;
    [SerializeField] private bool runtimeAdded;
    private AttackCooldown runtimeCooldown;
    private Func<float> timeProvider;
    #endregion

    #region Constructors
    public AbilitySlot()
    {
    }

    public AbilitySlot(Ability ability, float weightMultiplier, float cooldownOverride, bool runtimeAdded)
    {
        this.ability = ability;
        this.weightMultiplier = weightMultiplier;
        this.cooldownOverride = cooldownOverride;
        this.runtimeAdded = runtimeAdded;
    }
    #endregion

    #region Properties
    public Ability Ability => ability;
    public float Weight => (ability != null ? ability.UseWeight : 0f) * Mathf.Max(0.01f, weightMultiplier);
    public bool RuntimeAdded => runtimeAdded;
    #endregion

    #region Public Methods
    public void Initialize(Func<float> timeProvider = null)
    {
        if (timeProvider != null)
        {
            this.timeProvider = timeProvider;
        }

        float cooldown = cooldownOverride >= 0f
            ? cooldownOverride
            : (ability != null ? ability.CooldownSeconds : 0f);

        runtimeCooldown = new AttackCooldown(cooldown, this.timeProvider);
    }

    public bool IsReady()
    {
        if (ability == null)
        {
            return false;
        }

        if (runtimeCooldown == null)
        {
            Initialize();
        }

        return runtimeCooldown.IsReady();
    }

    public void MarkUsed()
    {
        if (runtimeCooldown == null)
        {
            Initialize();
        }

        runtimeCooldown.Reset();
    }
    #endregion
}
