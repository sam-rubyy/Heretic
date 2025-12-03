using UnityEngine;

[CreateAssetMenu(fileName = "StormCapacitor", menuName = "Items/Storm Capacitor")]
public class StormCapacitor : ItemBase, IShotModifier, IBulletModifier, IItemModifierPriority
{
    [SerializeField] private float fireRatePenalty = -0.4f;
    [SerializeField] private float burstIntervalReduction = -0.05f;
    [SerializeField] private float damageBonus = 6f;
    [SerializeField] private float knockbackBonus = 2f;
    [SerializeField] private int priority = 10;
    [Header("Chain Lightning")]
    [SerializeField] private float chainDamageMultiplier = 0.45f;
    [SerializeField] private float chainRange = 4f;
    [SerializeField] private int chainBounces = 3;
    [SerializeField] private float chainDamageFalloff = 0.75f;
    [SerializeField] private Color lightningTint = new Color(0.6f, 0.9f, 1f, 1f);

    public int Priority => priority;

    public ShotParams ModifyShot(ShotParams shotParams)
    {
        shotParams.fireRate = Mathf.Max(0f, shotParams.fireRate + fireRatePenalty);
        shotParams.burstInterval = Mathf.Max(0f, shotParams.burstInterval + burstIntervalReduction);
        return shotParams;
    }

    public BulletParams ModifyBullet(BulletParams bulletParams)
    {
        bulletParams.damage += damageBonus;
        bulletParams.knockback += knockbackBonus;
        bulletParams.chainLightning.enabled = true;
        bulletParams.chainLightning.damage = Mathf.Max(0f, bulletParams.damage * chainDamageMultiplier);
        bulletParams.chainLightning.maxBounces = Mathf.Max(0, chainBounces);
        bulletParams.chainLightning.range = Mathf.Max(0f, chainRange);
        bulletParams.chainLightning.damageFalloff = Mathf.Max(0f, chainDamageFalloff);
        bulletParams.tint = lightningTint;
        bulletParams.overrideTint = true;
        return bulletParams;
    }
}
