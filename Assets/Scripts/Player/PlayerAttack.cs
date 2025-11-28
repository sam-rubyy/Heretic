using UnityEngine;

[DisallowMultipleComponent]
public class PlayerAttack : MonoBehaviour
{
    #region Fields
    [SerializeField] private WeaponBase equippedWeapon;
    [SerializeField] private AttackCooldown attackCooldown = new AttackCooldown(0.25f);
    #endregion

    #region Unity Methods
    private void Update()
    {
    }
    #endregion

    #region Public Methods
    public void SetWeapon(WeaponBase weapon)
    {
        equippedWeapon = weapon;
    }

    public void TryAttack()
    {
    }
    #endregion
}
