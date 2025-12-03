using UnityEngine;

[CreateAssetMenu(fileName = "DamageBoostItem", menuName = "Items/Damage Boost")]
public class DamageBoostItem : ItemBase, IBulletModifier
{
    [SerializeField] private float DamagePercentageBonus = 1.25f;

    public BulletParams ModifyBullet(BulletParams bulletParams)
    {
        bulletParams.damage *= DamagePercentageBonus;
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
