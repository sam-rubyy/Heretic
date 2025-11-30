using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class WeaponBase : MonoBehaviour
{
    #region Fields
    [SerializeField] private WeaponData weaponData;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private ItemManager itemManager;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private BulletOwner bulletOwner = BulletOwner.Player;
    private AttackCooldown attackCooldown;
    private Coroutine burstRoutine;
    private bool isBursting;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        InitializeCooldown();
    }
    #endregion

    #region Public Methods
    public virtual void Initialize(WeaponData data)
    {
        weaponData = data;
        InitializeCooldown();
    }

    public virtual void HandleAttack()
    {
        UpdateCooldownFromStats();
        Vector2 direction = firePoint != null ? (Vector2)firePoint.right : (Vector2)transform.right;
        HandleAttack(direction);
    }

    public virtual void HandleAttack(Vector2 direction)
    {
        if (weaponData == null || bulletPrefab == null || firePoint == null)
        {
            return;
        }

        UpdateCooldownFromStats();

        if (!CanFire())
        {
            return;
        }

        var shotParams = GetModifiedShotParams(weaponData.ShotParameters);
        var bulletParams = GetModifiedBulletParams(weaponData.BulletParameters);
        if (shotParams.maxRange > 0f)
        {
            bulletParams.lifetime = shotParams.maxRange; // Treat lifetime as max travel distance.
        }

        int burstCount = Mathf.Max(1, shotParams.burstCount);

        if (burstCount <= 1)
        {
            FireVolley(direction, shotParams, bulletParams);
            attackCooldown?.Reset();
        }
        else
        {
            isBursting = true;
            if (burstRoutine != null)
            {
                StopCoroutine(burstRoutine);
            }

            burstRoutine = StartCoroutine(FireBurst(direction, shotParams, bulletParams));
        }
    }

    public virtual void Reload()
    {
        InitializeCooldown();
        attackCooldown?.Reset();
    }

    public bool CanFire()
    {
        if (isBursting)
        {
            return false;
        }

        UpdateCooldownFromStats();
        return attackCooldown == null || attackCooldown.IsReady();
    }
    #endregion

    #region Private Methods
    private void InitializeCooldown()
    {
        float cooldown = GetShotCooldownSeconds();
        attackCooldown = new AttackCooldown(cooldown);
    }

    private ShotParams GetModifiedShotParams(ShotParams baseParams)
    {
        if (playerStats != null)
        {
            baseParams = playerStats.ModifyShot(baseParams);
        }

        if (itemManager == null)
        {
            return baseParams;
        }

        return itemManager.ApplyShotModifiers(baseParams);
    }

    private BulletParams GetModifiedBulletParams(BulletParams baseParams)
    {
        if (playerStats != null)
        {
            baseParams = playerStats.ModifyBullet(baseParams);
        }

        if (itemManager == null)
        {
            return baseParams;
        }

        return itemManager.ApplyBulletModifiers(baseParams);
    }

    private float GetShotCooldownSeconds()
    {
        if (weaponData == null)
        {
            return 0f;
        }

        var finalShotParams = GetModifiedShotParams(weaponData.ShotParameters);

        // Treat fireRate as shots per second; cooldown is its inverse.
        float fireRate = finalShotParams.fireRate;
        if (fireRate <= 0f)
        {
            return 0f;
        }

        return 1f / fireRate;
    }

    private void FireVolley(Vector2 direction, ShotParams shotParams, BulletParams bulletParams)
    {
        int projectileCount = Mathf.Max(1, shotParams.projectilesPerShot);
        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        for (int i = 0; i < projectileCount; i++)
        {
            float spread = shotParams.spreadAngle;
            float t = projectileCount == 1 ? 0.5f : (float)i / (projectileCount - 1);
            float angleOffset = Mathf.Lerp(-spread * 0.5f, spread * 0.5f, t);
            float finalAngle = baseAngle + angleOffset;
            Quaternion rotation = Quaternion.Euler(0f, 0f, finalAngle);

            Bullet bulletInstance = Instantiate(bulletPrefab, firePoint.position, rotation);
            bulletInstance.SetOwner(bulletOwner);
            bulletInstance.Initialize(bulletParams, rotation * Vector2.right);
        }
    }

    private IEnumerator FireBurst(Vector2 direction, ShotParams shotParams, BulletParams bulletParams)
    {
        float interval = Mathf.Max(0f, shotParams.burstInterval);
        int burstCount = Mathf.Max(1, shotParams.burstCount);

        for (int i = 0; i < burstCount; i++)
        {
            FireVolley(direction, shotParams, bulletParams);
            if (i < burstCount - 1 && interval > 0f)
            {
                yield return new WaitForSeconds(interval);
            }
        }

        attackCooldown?.Reset();
        burstRoutine = null;
        isBursting = false;
    }

    private void UpdateCooldownFromStats()
    {
        if (attackCooldown == null)
        {
            InitializeCooldown();
            return;
        }

        attackCooldown.SetCooldown(GetShotCooldownSeconds());
    }
    #endregion
}
