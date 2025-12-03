using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileSpreadAbility", menuName = "Abilities/Projectile Spread")]
public class ProjectileSpreadAbility : Ability
{
    #region Fields
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private BulletParams bulletParams;
    [SerializeField] private Vector2 spawnOffset;
    [SerializeField] private int projectileCount = 3;
    [SerializeField] private float spreadAngle = 25f;
    [SerializeField] private bool requireTarget = true;
    #endregion

    #region Public Methods
    public override bool CanUse(AbilityContext context)
    {
        if (bulletPrefab == null || projectileCount <= 0)
        {
            return false;
        }

        if (requireTarget && !HasAimOrTarget(context))
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
        Vector2 baseDirection = ProjectileAbilityUtils.ResolveAimDirection(context, origin);
        int count = Mathf.Max(1, projectileCount);
        float totalSpread = Mathf.Max(0f, spreadAngle);
        float step = count > 1 ? totalSpread / (count - 1) : 0f;
        float start = -totalSpread * 0.5f;

        for (int i = 0; i < count; i++)
        {
            float angleOffset = start + step * i;
            Vector2 direction = ProjectileAbilityUtils.Rotate(baseDirection, angleOffset).normalized;
            Vector2 spawnPos = origin + ProjectileAbilityUtils.RotateOffset(spawnOffset, direction);

            SpawnBullet(spawnPos, direction, context);
        }
    }
    #endregion

    #region Private Methods
    private void SpawnBullet(Vector2 spawnPos, Vector2 direction, AbilityContext context)
    {
        Bullet bulletInstance = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        bulletInstance.Initialize(bulletParams, direction);
        bulletInstance.SetOwner(ProjectileAbilityUtils.GetOwnerFromContext(context));
    }
    #endregion
}
