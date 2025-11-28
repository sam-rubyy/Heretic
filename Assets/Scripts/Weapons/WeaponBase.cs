using UnityEngine;

[DisallowMultipleComponent]
public class WeaponBase : MonoBehaviour
{
    #region Fields
    [SerializeField] private WeaponData weaponData;
    [SerializeField] private Transform firePoint;
    private AttackCooldown attackCooldown;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        if (weaponData != null)
        {
            attackCooldown = new AttackCooldown(weaponData.ShotParameters.fireRate);
        }
    }
    #endregion

    #region Public Methods
    public virtual void Initialize(WeaponData data)
    {
        weaponData = data;
    }

    public virtual void HandleAttack()
    {
    }

    public virtual void Reload()
    {
    }
    #endregion
}
