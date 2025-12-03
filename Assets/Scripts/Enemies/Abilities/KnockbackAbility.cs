using UnityEngine;

[CreateAssetMenu(fileName = "KnockbackAbility", menuName = "Abilities/Knockback")]
public class KnockbackAbility : Ability
{
    #region Fields
    [SerializeField] private float radius = 2f;
    [SerializeField] private float force = 6f;
    [SerializeField] private LayerMask targetMask = ~0;
    [SerializeField] private int maxTargets = 4;
    [SerializeField] private bool radialFromUser = true;
    [SerializeField] private bool pullInsteadOfPush = false;
    [SerializeField] private bool requireTargetInRange = false;
    #endregion

    #region Public Methods
    public override bool CanUse(AbilityContext context)
    {
        if (radius <= 0f || force <= 0f)
        {
            return false;
        }

        if (requireTargetInRange && (context == null || context.Target == null || context.DistanceToTarget > radius))
        {
            return false;
        }

        return base.CanUse(context);
    }

    public override void Activate(AbilityContext context)
    {
        if (context == null || context.UserTransform == null)
        {
            return;
        }

        Vector2 origin = context.UserPosition;
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, radius, targetMask);
        int remaining = Mathf.Max(1, maxTargets);

        for (int i = 0; i < hits.Length && remaining > 0; i++)
        {
            var hit = hits[i];
            if (hit == null || hit.transform == context.UserTransform)
            {
                continue;
            }

            Vector2 pushDir = GetPushDirection(context, origin, hit.transform.position);
            if (pushDir.sqrMagnitude < 0.0001f)
            {
                continue;
            }

            if (ApplyKnockback(hit, pushDir, origin))
            {
                remaining--;
            }
        }
    }
    #endregion

    #region Private Methods
    private Vector2 GetPushDirection(AbilityContext context, Vector2 origin, Vector2 targetPosition)
    {
        Vector2 direction;
        if (radialFromUser)
        {
            direction = targetPosition - origin;
        }
        else
        {
            direction = context.AimDirection.sqrMagnitude > 0.0001f
                ? context.AimDirection
                : context.Target != null ? (context.TargetPosition - origin) : Vector2.right;
        }

        if (pullInsteadOfPush)
        {
            direction = -direction;
        }

        return direction;
    }

    private bool ApplyKnockback(Collider2D hit, Vector2 direction, Vector2 source)
    {
        if (hit == null)
        {
            return false;
        }

        var receiver = hit.GetComponentInParent<IKnockbackReceiver>();
        if (receiver != null)
        {
            receiver.ApplyKnockback(direction.normalized, force);
            return true;
        }

        var enemyHealth = hit.GetComponentInParent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.ApplyKnockback(source, force);
            return true;
        }

        var playerMovement = hit.GetComponentInParent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.ApplyKnockback(direction.normalized, force);
            return true;
        }

        if (hit.attachedRigidbody != null)
        {
            hit.attachedRigidbody.AddForce(direction.normalized * force, ForceMode2D.Impulse);
            return true;
        }

        return false;
    }
    #endregion
}
