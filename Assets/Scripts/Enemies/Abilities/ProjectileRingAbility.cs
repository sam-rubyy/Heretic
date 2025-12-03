using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileRingAbility", menuName = "Abilities/Projectile Ring")]
public class ProjectileRingAbility : Ability
{
    #region Fields
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private BulletParams bulletParams;
    [SerializeField] private Vector2 spawnOffset;
    [SerializeField] private float ringRadius = 0.5f;
    [SerializeField] private int projectileCount = 8;
    [SerializeField] private float initialAngleOffset = 0f;
    [SerializeField] private bool alignStartToAim = true;
    #endregion

    #region Public Methods
    public override bool CanUse(AbilityContext context)
    {
        if (bulletPrefab == null || projectileCount <= 0)
        {
            return false;
        }

        return base.CanUse(context);
    }

    public override void Activate(AbilityContext context)
    {
        if (context == null || context.User == null || bulletPrefab == null)
        {
            return;
        }

        Vector2 origin = context.UserPosition;
        Vector2 forward = ProjectileAbilityUtils.ResolveAimDirection(context, origin);
        float startAngle = initialAngleOffset;

        if (alignStartToAim && forward.sqrMagnitude > 0.0001f)
        {
            startAngle += Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg;
        }

        int count = Mathf.Max(1, projectileCount);
        float step = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + step * i;
            Vector2 direction = AngleToDirection(angle);
            Vector2 spawnPos = origin + direction * Mathf.Max(0f, ringRadius) + ProjectileAbilityUtils.RotateOffset(spawnOffset, direction);

            SpawnBullet(spawnPos, direction, context);
        }
    }
    #endregion

    #region Private Methods
    private static Vector2 AngleToDirection(float angleDegrees)
    {
        float radians = angleDegrees * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
    }

    private void SpawnBullet(Vector2 spawnPos, Vector2 direction, AbilityContext context)
    {
        Bullet bulletInstance = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        bulletInstance.Initialize(bulletParams, direction.normalized);
        bulletInstance.SetOwner(ProjectileAbilityUtils.GetOwnerFromContext(context));
    }
    #endregion
}
