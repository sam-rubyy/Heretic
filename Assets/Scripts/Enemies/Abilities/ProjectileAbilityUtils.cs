using UnityEngine;

public static class ProjectileAbilityUtils
{
    public static BulletOwner GetOwnerFromContext(AbilityContext context)
    {
        if (context?.User == null)
        {
            return BulletOwner.Neutral;
        }

        return context.User.CompareTag("Player") ? BulletOwner.Player : BulletOwner.Enemy;
    }

    public static Vector2 ResolveAimDirection(AbilityContext context, Vector2 spawnPosition)
    {
        Vector2 direction = Vector2.right;

        if (context != null)
        {
            if (context.Target != null)
            {
                direction = context.TargetPosition - spawnPosition;
            }
            else if (context.AimDirection.sqrMagnitude > 0.0001f)
            {
                direction = context.AimDirection;
            }
            else if (context.UserTransform != null)
            {
                direction = context.UserTransform.right;
            }
        }

        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = Vector2.right;
        }

        return direction.normalized;
    }

    public static Vector2 Rotate(Vector2 vector, float angleDegrees)
    {
        if (vector.sqrMagnitude < 0.0001f || Mathf.Abs(angleDegrees) <= 0.0001f)
        {
            return vector;
        }

        float radians = angleDegrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        float x = vector.x * cos - vector.y * sin;
        float y = vector.x * sin + vector.y * cos;

        return new Vector2(x, y);
    }

    public static Vector2 RotateOffset(Vector2 offset, Vector2 aimDirection)
    {
        if (offset.sqrMagnitude <= 0.0001f)
        {
            return Vector2.zero;
        }

        if (aimDirection.sqrMagnitude <= 0.0001f)
        {
            return offset;
        }

        var dir = aimDirection.normalized;
        float angle = Mathf.Atan2(dir.y, dir.x);
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);

        float x = offset.x * cos - offset.y * sin;
        float y = offset.x * sin + offset.y * cos;
        return new Vector2(x, y);
    }
}
