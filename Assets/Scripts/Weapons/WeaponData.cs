using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    #region Fields
    [SerializeField] private ShotParams shotParameters;
    [SerializeField] private BulletParams bulletParameters;
    #endregion

    #region Properties
    public ShotParams ShotParameters => shotParameters;
    public BulletParams BulletParameters => bulletParameters;
    #endregion
}

[System.Serializable]
public struct ShotParams
{
    [Header("Timing")]
    public float fireRate;

    [Header("Spread")]
    public float spreadAngle;
    public int projectilesPerShot;

    [Header("Range")]
    public float maxRange;
}

[System.Serializable]
public struct BulletParams
{
    [Header("Stats")]
    public int damage;
    public float speed;
    public float lifetime;
    public float knockback;
}
