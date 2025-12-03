using UnityEngine;

public abstract class Ability : ScriptableObject
{
    #region Fields
    [SerializeField] private string abilityId;
    [SerializeField] private float cooldownSeconds = 2f;
    [SerializeField] private float useWeight = 1f;
    [Header("Targeting")]
    [SerializeField] private float minRange = 0f;
    [SerializeField] private float maxRange = 9999f;
    #endregion

    #region Properties
    public string AbilityId => abilityId;
    public float CooldownSeconds => Mathf.Max(0f, cooldownSeconds);
    public float UseWeight => Mathf.Max(0.01f, useWeight);
    #endregion

    #region Public Methods
    public virtual bool CanUse(AbilityContext context)
    {
        if (context == null || context.User == null)
        {
            return false;
        }

        if (context.Target == null)
        {
            return true;
        }

        float distance = context.DistanceToTarget;
        return distance >= minRange && distance <= maxRange;
    }

    public abstract void Activate(AbilityContext context);
    #endregion

    #region Protected Methods
    protected bool HasAimOrTarget(AbilityContext context)
    {
        if (context == null)
        {
            return false;
        }

        return context.Target != null || context.AimDirection.sqrMagnitude > 0.0001f;
    }
    #endregion
}
