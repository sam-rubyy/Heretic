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
    public int burstCount;
    public float burstInterval;

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

    [Header("On Hit Effects")]
    public StatusEffectParams[] onHitEffects;

    [Header("On Travel Effects")]
    public TravelEffectParams[] onTravelEffects;
}

[System.Serializable]
public struct StatusEffectParams
{
    [Tooltip("Unique identifier for the status effect, e.g. Burning, Freeze.")]
    public string effectId;
    [Range(0f, 1f)] public float chance;
    public float duration;
    public float intensity;
}

[System.Serializable]
public struct TravelEffectParams
{
    [Tooltip("Unique identifier for the travel effect, e.g. FireTrail.")]
    public string effectId;
    [Range(0f, 1f)] public float chance;
    public float tickInterval;
    public float intensity;
}
