using UnityEngine;

[CreateAssetMenu(fileName = "DamageBoostItem", menuName = "Items/Damage Boost")]
public class DamageBoostItem : ItemBase, IBulletModifier
{
    [SerializeField] private int flatDamageBonus = 5;

    public BulletParams ModifyBullet(BulletParams bulletParams)
    {
        bulletParams.damage += flatDamageBonus;
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
