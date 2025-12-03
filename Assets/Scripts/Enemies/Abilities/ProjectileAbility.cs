using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileAbility", menuName = "Abilities/Projectile")]
public class ProjectileAbility : Ability
{
    #region Fields
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private BulletParams bulletParams;
    [SerializeField] private Vector2 spawnOffset;
    [SerializeField] private float aimSpreadDegrees = 0f;
    [SerializeField] private bool requireTarget = true;
    #endregion

    #region Public Methods
    public override bool CanUse(AbilityContext context)
    {
        if (requireTarget && !HasAimOrTarget(context))
        {
            return false;
        }

        return base.CanUse(context);
    }

    public override void Activate(AbilityContext context)
    {
        if (bulletPrefab == null || context == null || context.User == null)
        {
            return;
        }

        Vector2 aimDirection = GetAimDirection(context, context.UserPosition);
        Vector2 spawnPosition = context.UserPosition + GetAimedOffset(aimDirection);

        Bullet bulletInstance = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
        bulletInstance.Initialize(bulletParams, aimDirection);
        bulletInstance.SetOwner(GetOwnerFromContext(context));
    }
    #endregion

    #region Private Methods
    private BulletOwner GetOwnerFromContext(AbilityContext context)
    {
        if (context?.User == null)
        {
            return BulletOwner.Neutral;
        }

        return context.User.CompareTag("Player") ? BulletOwner.Player : BulletOwner.Enemy;
    }

    private Vector2 GetAimDirection(AbilityContext context, Vector2 spawnPosition)
    {
        Vector2 direction;

        if (context.Target != null)
        {
            direction = context.TargetPosition - spawnPosition;
        }
        else
        {
            direction = context.AimDirection.sqrMagnitude > 0.0001f
                ? context.AimDirection
                : context.User != null ? (Vector2)context.User.transform.right : Vector2.right;
        }

        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = Vector2.right;
        }

        direction = ApplySpread(direction.normalized);
        return direction.normalized;
    }

    private Vector2 GetAimedOffset(Vector2 aimDirection)
    {
        if (spawnOffset.sqrMagnitude <= 0.0001f)
        {
            return Vector2.zero;
        }

        var dir = aimDirection.sqrMagnitude > 0.0001f ? aimDirection.normalized : Vector2.right;
        float angle = Mathf.Atan2(dir.y, dir.x);
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);

        float x = spawnOffset.x * cos - spawnOffset.y * sin;
        float y = spawnOffset.x * sin + spawnOffset.y * cos;
        return new Vector2(x, y);
    }

    private Vector2 ApplySpread(Vector2 direction)
    {
        if (aimSpreadDegrees <= 0.01f)
        {
            return direction;
        }

        float halfSpread = aimSpreadDegrees * 0.5f;
        float angleOffset = Random.Range(-halfSpread, halfSpread);
        float angleRadians = angleOffset * Mathf.Deg2Rad;

        float cos = Mathf.Cos(angleRadians);
        float sin = Mathf.Sin(angleRadians);

        float x = direction.x * cos - direction.y * sin;
        float y = direction.x * sin + direction.y * cos;

        return new Vector2(x, y);
    }
    #endregion
}
