using UnityEngine;

[CreateAssetMenu(fileName = "HeavyPayload", menuName = "Items/Heavy Payload")]
public class HeavyPayload : ItemBase, IBulletModifier
{
    [SerializeField] private float damageBonus = 1.5f;
    [SerializeField] private float speedMultiplier = 0.75f;
    [SerializeField] private float knockbackBonus = 4f;

    public BulletParams ModifyBullet(BulletParams bulletParams)
    {
        bulletParams.damage *= damageBonus;
        bulletParams.knockback += knockbackBonus;
        bulletParams.speed = Mathf.Max(0f, bulletParams.speed * speedMultiplier);
        return bulletParams;
    }
}
