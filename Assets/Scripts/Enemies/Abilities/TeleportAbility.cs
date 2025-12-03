using UnityEngine;

[CreateAssetMenu(fileName = "TeleportAbility", menuName = "Abilities/Teleport")]
public class TeleportAbility : Ability
{
    #region Fields
    [SerializeField] private float teleportDistance = 4f;
    [SerializeField] private float targetOffset = 1f;
    [SerializeField] private bool snapNearTarget = true;
    [SerializeField] private bool clearVelocityOnTeleport = true;
    [SerializeField] private bool requireTarget = false;
    #endregion

    #region Public Methods
    public override bool CanUse(AbilityContext context)
    {
        if (teleportDistance <= 0f)
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
        if (context == null || context.UserTransform == null)
        {
            return;
        }

        Vector2 direction = ResolveDirection(context);
        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = Vector2.right;
        }

        Vector2 destination = context.UserPosition + direction.normalized * teleportDistance;
        if (snapNearTarget && context.Target != null)
        {
            destination = context.TargetPosition - direction.normalized * targetOffset;
        }

        context.UserTransform.position = destination;
        if (clearVelocityOnTeleport && context.User.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.velocity = Vector2.zero;
        }
    }
    #endregion

    #region Private Methods
    private Vector2 ResolveDirection(AbilityContext context)
    {
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
    #endregion
}
