using UnityEngine;

[CreateAssetMenu(fileName = "TorrentialDownpour", menuName = "Items/TorrentialDownpour")]
public class MinigunItem : ItemBase, IShotModifier, IBulletModifier, IItemModifierPriority
{
    [SerializeField] private int damagePenalty = 8;
    [SerializeField] private float fireRateBonus = 10f;

    // Apply this item after other damage boosts so the penalty sticks.
    public int Priority => 100;

    public ShotParams ModifyShot(ShotParams shotParams)
    {
        shotParams.fireRate += fireRateBonus;
        return shotParams;
    }

    public BulletParams ModifyBullet(BulletParams bulletParams)
    {
        int divisor = Mathf.Max(1, damagePenalty);
        bulletParams.damage = bulletParams.damage / divisor;
        return bulletParams;
    }

    public override void OnCollected(GameObject collector)
    {
        // Optional: VFX/SFX/notify UI
    }

    public override void OnRemoved(GameObject collector)
    {
        // Optional: cleanup
    }
}
