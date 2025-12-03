using UnityEngine;

[CreateAssetMenu(fileName = "ConeSlashAbility", menuName = "Abilities/Cone Slash")]
public class ConeSlashAbility : Ability
{
    #region Fields
    [SerializeField] private float radius = 2f;
    [SerializeField] private float angleDegrees = 90f;
    [SerializeField] private float damage = 1f;
    [SerializeField] private float knockbackForce = 3f;
    [SerializeField] private LayerMask targetMask = ~0;
    [SerializeField] private bool requireTarget = false;
    [SerializeField] private int maxTargets = 3;
    #endregion

    #region Public Methods
    public override bool CanUse(AbilityContext context)
    {
        if (radius <= 0f || damage <= 0f || angleDegrees <= 0f)
        {
            return false;
        }

        if (requireTarget && (context == null || context.Target == null))
        {
            return false;
        }

        return base.CanUse(context);
    }

    public override void Activate(AbilityContext context)
    {
        if (context == null || context.User == null)
        {
            return;
        }

        Vector2 origin = context.UserPosition;
        Vector2 forward = ResolveAimDirection(context);
        if (forward.sqrMagnitude < 0.0001f)
        {
            forward = Vector2.right;
        }

        float cosThreshold = Mathf.Cos(0.5f * angleDegrees * Mathf.Deg2Rad);
        bool userIsPlayer = context.User.CompareTag("Player");
        int remaining = Mathf.Max(1, maxTargets);

        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, radius, targetMask);
        for (int i = 0; i < hits.Length && remaining > 0; i++)
        {
            var hit = hits[i];
            if (hit == null)
            {
                continue;
            }

            Vector2 toHit = (Vector2)hit.transform.position - origin;
            if (toHit.sqrMagnitude < 0.0001f)
            {
                continue;
            }

            Vector2 dir = toHit.normalized;
            float dot = Vector2.Dot(forward.normalized, dir);
            if (dot < cosThreshold)
            {
                continue;
            }

            if (ApplyToCollider(hit, origin, userIsPlayer))
            {
                remaining--;
            }
        }
    }
    #endregion

    #region Private Methods
    private Vector2 ResolveAimDirection(AbilityContext context)
    {
        if (context == null)
        {
            return Vector2.right;
        }

        if (context.Target != null)
        {
            Vector2 toTarget = context.TargetPosition - context.UserPosition;
            if (toTarget.sqrMagnitude > 0.0001f)
            {
                return toTarget.normalized;
            }
        }

        if (context.AimDirection.sqrMagnitude > 0.0001f)
        {
            return context.AimDirection.normalized;
        }

        return context.UserTransform != null ? (Vector2)context.UserTransform.right : Vector2.right;
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
