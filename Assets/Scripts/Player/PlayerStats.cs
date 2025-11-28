using UnityEngine;

[DisallowMultipleComponent]
public class PlayerStats : MonoBehaviour
{
    #region Fields
    [Header("Shot Modifiers")]
    [SerializeField] private float fireRateMultiplier = 1f;
    [SerializeField] private float fireRateBonus;
    [SerializeField] private int burstCountBonus;
    [SerializeField] private float spreadAngleBonus;
    [SerializeField] private int projectilesPerShotBonus;
    [SerializeField] private float maxRangeBonus;

    [Header("Bullet Modifiers")]
    [SerializeField] private float damageMultiplier = 1f;
    [SerializeField] private int damageBonus;
    [SerializeField] private float speedMultiplier = 1f;
    [SerializeField] private float speedBonus;
    [SerializeField] private float lifetimeBonus;
    [SerializeField] private float knockbackMultiplier = 1f;
    [SerializeField] private float knockbackBonus;
    #endregion

    #region Public Methods
    public ShotParams ModifyShot(ShotParams shotParams)
    {
        shotParams.fireRate = Mathf.Max(0f, shotParams.fireRate * fireRateMultiplier + fireRateBonus);
        shotParams.burstCount = Mathf.Max(1, shotParams.burstCount + burstCountBonus);
        shotParams.spreadAngle = Mathf.Max(0f, shotParams.spreadAngle + spreadAngleBonus);
        shotParams.projectilesPerShot = Mathf.Max(1, shotParams.projectilesPerShot + projectilesPerShotBonus);
        shotParams.maxRange = Mathf.Max(0f, shotParams.maxRange + maxRangeBonus);
        return shotParams;
    }

    public BulletParams ModifyBullet(BulletParams bulletParams)
    {
        bulletParams.damage = Mathf.Max(0, Mathf.RoundToInt(bulletParams.damage * damageMultiplier) + damageBonus);
        bulletParams.speed = Mathf.Max(0f, bulletParams.speed * speedMultiplier + speedBonus);
        bulletParams.lifetime = Mathf.Max(0f, bulletParams.lifetime + lifetimeBonus);
        bulletParams.knockback = Mathf.Max(0f, bulletParams.knockback * knockbackMultiplier + knockbackBonus);
        return bulletParams;
    }

    public void AddFireRateMultiplier(float amount) => fireRateMultiplier += amount;
    public void AddFireRateBonus(float amount) => fireRateBonus += amount;
    public void AddBurstCountBonus(int amount) => burstCountBonus += amount;
    public void AddSpreadAngleBonus(float amount) => spreadAngleBonus += amount;
    public void AddProjectilesPerShotBonus(int amount) => projectilesPerShotBonus += amount;
    public void AddMaxRangeBonus(float amount) => maxRangeBonus += amount;

    public void AddDamageMultiplier(float amount) => damageMultiplier += amount;
    public void AddDamageBonus(int amount) => damageBonus += amount;
    public void AddSpeedMultiplier(float amount) => speedMultiplier += amount;
    public void AddSpeedBonus(float amount) => speedBonus += amount;
    public void AddLifetimeBonus(float amount) => lifetimeBonus += amount;
    public void AddKnockbackMultiplier(float amount) => knockbackMultiplier += amount;
    public void AddKnockbackBonus(float amount) => knockbackBonus += amount;
    #endregion
}
