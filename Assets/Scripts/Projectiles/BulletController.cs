using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Bullet))]
public class BulletController : MonoBehaviour
{
    #region Fields
    [SerializeField] private Bullet bullet;
    [Header("Travel Effect Prefabs")]
    [SerializeField] private GameObject fireTrailPrefab;
    [SerializeField] private float fireTrailLifetime = 1.5f;
    [SerializeField] private float fireTrailTickInterval = 0.25f;
    [SerializeField] private float fireTrailRadius = 0.6f;
    private float distanceTraveled;
    private BulletParams cachedParams;
    private bool hasCachedParams;
    private readonly Dictionary<string, float> travelEffectTimers = new Dictionary<string, float>();
    #endregion

    #region Unity Methods
    private void Awake()
    {
        if (bullet == null)
        {
            bullet = GetComponent<Bullet>();
        }
    }

    private void Update()
    {
        if (!EnsureParamsCached())
        {
            return;
        }

        HandleMovement();
        HandleTravelEffects();
        HandleLifetime();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleCollision(other);
    }
    #endregion

    #region Private Methods
    private void HandleMovement()
    {
        if (!EnsureParamsCached())
        {
            return;
        }

        Vector2 direction = bullet.GetMoveDirection();

        if (direction.sqrMagnitude < 0.001f)
        {
            direction = transform.right;
        }

        float deltaTime = GetDeltaTime();
        Vector2 displacement = direction.normalized * cachedParams.speed * deltaTime;
        transform.position += (Vector3)displacement;
        distanceTraveled += displacement.magnitude;
    }

    private void HandleCollision(Collider2D other)
    {
        if (!EnsureParamsCached())
        {
            return;
        }

        if (other == null)
        {
            return;
        }

        if (other.GetComponentInParent<Bullet>() != null)
        {
            return; // Ignore bullet-on-bullet collisions to prevent self-destruction in spreads/bursts.
        }

        bool hitEnemy;
        bool shouldDestroy = ApplyDamage(other, out hitEnemy);

        if (hitEnemy)
        {
            ProcessOnHitEffects(other);
            ProcessChainLightning(other.transform);
        }

        if (shouldDestroy)
        {
            Destroy(gameObject);
        }
    }

    private bool ApplyDamage(Collider2D other, out bool hitEnemy)
    {
        hitEnemy = false;

        if (!EnsureParamsCached())
        {
            return false;
        }

        BulletOwner owner = bullet.GetOwner();

        var playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth != null)
        {
            if (owner == BulletOwner.Player)
            {
                return false; // Ignore friendly fire on the player.
            }

            Vector2 sourcePos = transform.position;
            playerHealth.TakeDamage(Mathf.RoundToInt(cachedParams.damage), sourcePos, cachedParams.knockback);
            return true;
        }

        var enemyHealth = other.GetComponentInParent<EnemyHealth>();
        if (enemyHealth != null)
        {
            if (owner == BulletOwner.Enemy)
            {
                return false; // Ignore enemy bullets hitting enemies.
            }

            Vector2 sourcePos = transform.position;

            enemyHealth.TakeDamage(cachedParams.damage);
            enemyHealth.ApplyKnockback(sourcePos, cachedParams.knockback);

            if (cachedParams.knockback > 0f && other.attachedRigidbody != null && !other.TryGetComponent<IKnockbackReceiver>(out _))
            {
                Vector2 direction = bullet.GetMoveDirection();
                if (direction.sqrMagnitude < 0.001f)
                {
                    direction = transform.right;
                }

                other.attachedRigidbody.AddForce(direction.normalized * cachedParams.knockback, ForceMode2D.Impulse);
            }

            hitEnemy = true;
            return true;
        }

        // Hit environment or unknown target: destroy the bullet to avoid endless travel.
        return true;
    }

    private void ProcessOnHitEffects(Collider2D other)
    {
        var effects = bullet.GetOnHitEffects();

        if (effects == null || effects.Length == 0)
        {
            return;
        }

        var enemyHealth = other.GetComponentInParent<EnemyHealth>();
        if (enemyHealth == null)
        {
            return;
        }

        foreach (var effect in effects)
        {
            float chance = Mathf.Clamp01(effect.chance);
            if (chance <= 0f)
            {
                continue;
            }

            if (Random.value <= chance)
            {
                enemyHealth.ApplyStatusEffect(effect);
            }
        }
    }

    private void HandleTravelEffects()
    {
        var effects = bullet.GetOnTravelEffects();
        if (effects == null || effects.Length == 0)
        {
            return;
        }

        float deltaTime = GetDeltaTime();

        for (int i = 0; i < effects.Length; i++)
        {
            var effect = effects[i];
            if (effect.chance <= 0f)
            {
                continue;
            }

            string id = effect.effectId != null ? effect.effectId.Trim().ToLowerInvariant() : string.Empty;
            if (string.IsNullOrWhiteSpace(id))
            {
                continue;
            }

            float interval = Mathf.Max(0.01f, effect.tickInterval);
            float timer = travelEffectTimers.ContainsKey(id) ? travelEffectTimers[id] : 0f;

            timer += deltaTime;
            while (timer >= interval)
            {
                timer -= interval;
                if (Random.value <= Mathf.Clamp01(effect.chance))
                {
                    ApplyTravelEffect(id, effect);
                }
            }

            travelEffectTimers[id] = timer;
        }
    }

    private void HandleLifetime()
    {
        if (!EnsureParamsCached())
        {
            return;
        }

        float maxDistance = cachedParams.lifetime;
        if (maxDistance <= 0f)
        {
            return;
        }

        if (distanceTraveled >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

    private bool EnsureParamsCached()
    {
        if (hasCachedParams)
        {
            return true;
        }

        if (bullet == null)
        {
            return false;
        }

        cachedParams = bullet.GetParameters();
        hasCachedParams = true;
        return true;
    }

    private float GetDeltaTime()
    {
        if (bullet != null && bullet.GetOwner() == BulletOwner.Player && BulletTimeRunner.UseUnscaledPlayerTime)
        {
            return BulletTimeRunner.GetPlayerDeltaTime();
        }

        return Time.deltaTime;
    }

    private void ProcessChainLightning(Transform hitTransform)
    {
        if (!EnsureParamsCached() || hitTransform == null)
        {
            return;
        }

        var lightning = cachedParams.chainLightning;
        if (!lightning.enabled || lightning.maxBounces <= 0 || lightning.range <= 0f || lightning.damage <= 0f)
        {
            return;
        }

        // Only chain from player-owned bullets to enemies to avoid punishing the player with enemy chains.
        if (bullet != null && bullet.GetOwner() != BulletOwner.Player)
        {
            return;
        }

        var firstEnemy = hitTransform.GetComponentInParent<EnemyHealth>();
        if (firstEnemy == null)
        {
            return;
        }

        var visited = new HashSet<EnemyHealth> { firstEnemy };
        Vector2 origin = hitTransform.position;
        float damage = lightning.damage;
        float falloff = lightning.damageFalloff <= 0f ? 1f : lightning.damageFalloff;

        for (int i = 0; i < lightning.maxBounces; i++)
        {
            var next = FindNearestEnemy(origin, lightning.range, visited);
            if (next == null)
            {
                break;
            }

            next.TakeDamage(damage);
            visited.Add(next);
            origin = next.transform.position;
            damage *= falloff;
        }
    }

    private EnemyHealth FindNearestEnemy(Vector2 origin, float range, HashSet<EnemyHealth> visited)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, range);
        EnemyHealth nearest = null;
        float bestSqr = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            var hit = hits[i];
            if (hit == null)
            {
                continue;
            }

            var enemyHealth = hit.GetComponentInParent<EnemyHealth>();
            if (enemyHealth == null || visited.Contains(enemyHealth))
            {
                continue;
            }

            float sqrDist = ((Vector2)enemyHealth.transform.position - origin).sqrMagnitude;
            if (sqrDist < bestSqr)
            {
                bestSqr = sqrDist;
                nearest = enemyHealth;
            }
        }

        return nearest;
    }

    private void ApplyTravelEffect(string id, TravelEffectParams effect)
    {
        switch (id)
        {
            case "firetrail":
            case "fire trail":
                SpawnFireTrail(effect);
                break;
            default:
                break;
        }
    }

    #region Fire Trail
    private void SpawnFireTrail(TravelEffectParams effect)
    {
        Vector2 position = transform.position;
        Quaternion rotation = transform.rotation;

        GameObject instance = null;
        if (fireTrailPrefab != null)
        {
            instance = Instantiate(fireTrailPrefab, position, rotation);
        }
        else
        {
            instance = new GameObject("FireTrail");
            instance.transform.position = position;
            instance.transform.rotation = rotation;
        }

        var trail = instance.GetComponent<FireTrailEffect>();
        if (trail == null)
        {
            trail = instance.AddComponent<FireTrailEffect>();
        }

        float damage = Mathf.Max(0f, effect.intensity);
        trail.Initialize(
            damage,
            fireTrailTickInterval,
            fireTrailLifetime,
            fireTrailRadius,
            bullet != null ? bullet.GetOwner() : BulletOwner.Neutral);
    }
    #endregion
    #endregion
}
