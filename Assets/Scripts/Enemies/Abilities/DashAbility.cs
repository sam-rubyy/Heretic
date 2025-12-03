using UnityEngine;

[CreateAssetMenu(fileName = "DashAbility", menuName = "Abilities/Dash")]
public class DashAbility : Ability
{
    #region Fields
    [SerializeField] private float dashSpeedMultiplier = 2f;
    [SerializeField] private float dashDurationSeconds = 0.2f;
    [SerializeField] private bool useUnscaledTime = true;
    #endregion

    #region Public Methods
    public override bool CanUse(AbilityContext context)
    {
        if (dashSpeedMultiplier <= 0f || dashDurationSeconds <= 0f)
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

        Vector2 direction = GetDashDirection(context);
        var runner = context.User.GetComponent<DashRunner>();
        if (runner == null)
        {
            runner = context.User.gameObject.AddComponent<DashRunner>();
        }

        runner.Trigger(direction, dashSpeedMultiplier, dashDurationSeconds, useUnscaledTime);
    }
    #endregion

    #region Private Methods
    private Vector2 GetDashDirection(AbilityContext context)
    {
        if (context == null || context.User == null)
        {
            return Vector2.right;
        }

        var movement = context.User.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            Vector2 moveDir = movement.GetMovementInputDirection();
            if (moveDir.sqrMagnitude > 0.0001f)
            {
                return moveDir.normalized;
            }
        }

        if (context.AimDirection.sqrMagnitude > 0.0001f)
        {
            return context.AimDirection.normalized;
        }

        var rb = context.User.GetComponent<Rigidbody2D>();
        if (rb != null && rb.velocity.sqrMagnitude > 0.0001f)
        {
            return rb.velocity.normalized;
        }

        return context.User.transform.right;
    }
    #endregion
}
