using UnityEngine;

[CreateAssetMenu(fileName = "StatusEffectAbility", menuName = "Abilities/Status Effect")]
public class StatusEffectAbility : Ability
{
    #region Fields
    [SerializeField] private StatusEffectParams effect = new StatusEffectParams
    {
        effectId = "Haste",
        chance = 1f,
        duration = 3f,
        intensity = 1f
    };
    [SerializeField] private bool applyToUser = true;
    [SerializeField] private bool applyToTarget = false;
    [SerializeField] private bool applyToAlliesInRadius = false;
    [SerializeField] private float radius = 3f;
    [SerializeField] private LayerMask allyMask = ~0;
    [SerializeField] private int maxAllies = 3;
    [SerializeField] private bool requireTargetWhenApplyingToTarget = true;
    #endregion

    #region Public Methods
    public override bool CanUse(AbilityContext context)
    {
        if (effect.duration <= 0f || effect.intensity <= 0f || string.IsNullOrWhiteSpace(effect.effectId))
        {
            return false;
        }

        if (applyToTarget && requireTargetWhenApplyingToTarget && (context == null || context.Target == null))
        {
            return false;
        }

        return base.CanUse(context);
    }

    public override void Activate(AbilityContext context)
    {
        if (context == null)
        {
            return;
        }

        if (applyToUser)
        {
            TryApplyEffect(context.User);
        }

        if (applyToTarget && context.Target != null)
        {
            TryApplyEffect(context.Target);
        }

        if (applyToAlliesInRadius && context.User != null)
        {
            ApplyToNearbyAllies(context);
        }
    }
    #endregion

    #region Private Methods
    private void ApplyToNearbyAllies(AbilityContext context)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(context.UserPosition, radius, allyMask);
        int remaining = Mathf.Max(1, maxAllies);

        for (int i = 0; i < hits.Length && remaining > 0; i++)
        {
            var hit = hits[i];
            if (hit == null)
            {
                continue;
            }

            if (hit.transform == context.UserTransform || hit.transform == context.Target)
            {
                continue;
            }

            if (TryApplyEffect(hit))
            {
                remaining--;
            }
        }
    }

    private bool TryApplyEffect(Component target)
    {
        if (target == null)
        {
            return false;
        }

        var enemyHealth = target.GetComponentInParent<EnemyHealth>();
        if (enemyHealth == null)
        {
            return false;
        }

        float chance = Mathf.Clamp01(effect.chance);
        if (chance > 0f && Random.value <= chance)
        {
            enemyHealth.ApplyStatusEffect(effect);
            return true;
        }

        return false;
    }
    #endregion
}
