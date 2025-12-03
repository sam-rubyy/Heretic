using UnityEngine;

[CreateAssetMenu(fileName = "AdrenalineRush", menuName = "Items/Adrenaline Rush")]
public class AdrenalineRush : ItemBase, IPlayerStatModifier
{
    [SerializeField] private float fireRateMultiplierBonus = 0.35f;
    [SerializeField] private float fireRateBonus = 0.65f;
    [SerializeField] private float damageMultiplierPenalty = -0.15f;

    public void Apply(PlayerStats stats)
    {
        if (stats == null)
        {
            return;
        }

        stats.AddFireRateMultiplier(fireRateMultiplierBonus);
        stats.AddFireRateBonus(fireRateBonus);
        stats.AddDamageMultiplier(damageMultiplierPenalty);
    }

    public void Remove(PlayerStats stats)
    {
        if (stats == null)
        {
            return;
        }

        stats.AddFireRateMultiplier(-fireRateMultiplierBonus);
        stats.AddFireRateBonus(-fireRateBonus);
        stats.AddDamageMultiplier(-damageMultiplierPenalty);
    }
}
