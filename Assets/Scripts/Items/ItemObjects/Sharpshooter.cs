using UnityEngine;

[CreateAssetMenu(fileName = "Sharpshooter", menuName = "Items/Sharpshooter")]
public class Sharpshooter : ItemBase, IPlayerStatModifier
{
    [SerializeField] private float damageMultiplierBonus = 0.35f;
    [SerializeField] private int damageBonus = 2;
    [SerializeField] private float projectileSpeedBonus = 1.5f;
    [SerializeField] private float rangeBonus = 4f;
    [SerializeField] private float spreadReduction = 4f;

    public void Apply(PlayerStats stats)
    {
        if (stats == null)
        {
            return;
        }

        stats.AddDamageMultiplier(damageMultiplierBonus);
        stats.AddDamageBonus(damageBonus);
        stats.AddSpeedBonus(projectileSpeedBonus);
        stats.AddMaxRangeBonus(rangeBonus);
        stats.AddSpreadAngleBonus(-Mathf.Abs(spreadReduction));
    }

    public void Remove(PlayerStats stats)
    {
        if (stats == null)
        {
            return;
        }

        stats.AddDamageMultiplier(-damageMultiplierBonus);
        stats.AddDamageBonus(-damageBonus);
        stats.AddSpeedBonus(-projectileSpeedBonus);
        stats.AddMaxRangeBonus(-rangeBonus);
        stats.AddSpreadAngleBonus(Mathf.Abs(spreadReduction));
    }
}
