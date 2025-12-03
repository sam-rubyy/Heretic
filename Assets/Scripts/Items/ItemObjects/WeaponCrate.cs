using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponCrate", menuName = "Items/Weapon Crate")]
public class WeaponCrate : ItemBase
{
    [SerializeField] private WeaponData weaponDataOverride;

    private readonly Dictionary<GameObject, WeaponData> previousWeaponData = new Dictionary<GameObject, WeaponData>();

    public override void OnCollected(GameObject collector)
    {
        if (collector == null)
        {
            return;
        }

        var attack = collector.GetComponentInChildren<PlayerAttack>();
        if (attack == null)
        {
            return;
        }

        var weapon = GetTargetWeapon(collector, attack);
        var replacementData = GetReplacementData();
        if (weapon == null || replacementData == null)
        {
            return;
        }

        previousWeaponData[collector] = weapon.GetWeaponData();
        weapon.Initialize(replacementData);
        attack.SetWeapon(weapon);
    }

    public override void OnRemoved(GameObject collector)
    {
        if (collector == null)
        {
            return;
        }

        var attack = collector.GetComponentInChildren<PlayerAttack>();
        if (attack == null)
        {
            return;
        }

        if (previousWeaponData.TryGetValue(collector, out var previousData))
        {
            var weapon = GetTargetWeapon(collector, attack);
            if (weapon != null && previousData != null)
            {
                weapon.Initialize(previousData);
                attack.SetWeapon(weapon);
            }

            previousWeaponData.Remove(collector);
        }
    }

    private WeaponBase GetTargetWeapon(GameObject collector, PlayerAttack attack)
    {
        return attack != null ? attack.GetComponentInChildren<WeaponBase>() : collector.GetComponentInChildren<WeaponBase>();
    }

    private WeaponData GetReplacementData()
    {
        if (weaponDataOverride != null)
        {
            return weaponDataOverride;
        }

        return null;
    }
}
