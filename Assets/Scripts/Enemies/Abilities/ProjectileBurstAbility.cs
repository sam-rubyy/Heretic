using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileBurstAbility", menuName = "Abilities/Projectile Burst")]
public class ProjectileBurstAbility : Ability
{
    #region Fields
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private BulletParams bulletParams;
    [SerializeField] private Vector2 spawnOffset;
    [SerializeField] private int shotsPerBurst = 3;
    [SerializeField] private float perShotSpread = 6f;
    [SerializeField] private float angleStepPerShot = 0f;
    [SerializeField] private bool requireTarget = true;
    #endregion

    #region Public Methods
    public override bool CanUse(AbilityContext context)
    {
        if (bulletPrefab == null || shotsPerBurst <= 0)
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
        int shots = Mathf.Max(1, shotsPerBurst);

        for (int i = 0; i < shots; i++)
        {
            float angleOffset = angleStepPerShot * i;
            if (perShotSpread > 0.01f)
            {
                float halfSpread = perShotSpread * 0.5f;
                angleOffset += Random.Range(-halfSpread, halfSpread);
            }

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
