using UnityEngine;

[CreateAssetMenu(fileName = "MeleeStrikeAbility", menuName = "Abilities/Melee Strike")]
public class MeleeStrikeAbility : Ability
{
    #region Fields
    [SerializeField] private float damage = 1f;
    [SerializeField] private float knockbackForce = 4f;
    [SerializeField] private float hitRadius = 1f;
    [SerializeField] private LayerMask targetMask = ~0;
    [SerializeField] private bool requireTargetInRange = true;
    [SerializeField] private int maxTargets = 1;
    #endregion

    #region Public Methods
    public override bool CanUse(AbilityContext context)
    {
        if (damage <= 0f || hitRadius <= 0f)
        {
            return false;
        }

        if (!base.CanUse(context))
        {
            return false;
        }

        if (!requireTargetInRange || context?.Target == null)
        {
            return true;
        }

        return context.DistanceToTarget <= hitRadius;
    }

    public override void Activate(AbilityContext context)
    {
        if (context == null || context.User == null)
        {
            return;
        }

        Vector2 origin = context.UserPosition;
        bool userIsPlayer = context.User.CompareTag("Player");
        int remainingHits = Mathf.Max(1, maxTargets);

        if (context.Target != null)
        {
            remainingHits -= ApplyToTransform(context.Target, origin, userIsPlayer) ? 1 : 0;
        }

        if (remainingHits <= 0)
        {
            return;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, hitRadius, targetMask);
        for (int i = 0; i < hits.Length && remainingHits > 0; i++)
        {
            var hit = hits[i];
            if (hit == null)
            {
                continue;
            }

            if (context.Target != null && hit.transform == context.Target)
            {
                continue;
            }

            if (ApplyToCollider(hit, origin, userIsPlayer))
            {
                remainingHits--;
            }
        }
    }
    #endregion

    #region Private Methods
    private bool ApplyToTransform(Transform target, Vector2 source, bool userIsPlayer)
    {
        if (target == null)
        {
            return false;
        }

        var collider = target.GetComponentInChildren<Collider2D>();
        if (collider != null)
        {
            return ApplyToCollider(collider, source, userIsPlayer);
        }

        return false;
    }

    private bool ApplyToCollider(Collider2D hit, Vector2 source, bool userIsPlayer)
    {
        if (hit == null)
        {
            return false;
        }

        if (userIsPlayer)
        {
            var enemyHealth = hit.GetComponentInParent<EnemyHealth>();
            if (enemyHealth == null)
            {
                return false;
            }

            enemyHealth.TakeDamage(damage);
            enemyHealth.ApplyKnockback(source, knockbackForce);
            return true;
        }
        else
        {
            var playerHealth = hit.GetComponentInParent<PlayerHealth>();
            if (playerHealth == null)
            {
                return false;
            }

            playerHealth.TakeDamage(Mathf.RoundToInt(damage), source, knockbackForce);
            return true;
        }
    }
    #endregion
}
