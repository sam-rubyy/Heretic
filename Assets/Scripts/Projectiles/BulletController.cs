using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Bullet))]
public class BulletController : MonoBehaviour
{
    #region Fields
    [SerializeField] private Bullet bullet;
    private float distanceTraveled;
    private float travelEffectTimer;
    private BulletParams cachedParams;
    private bool hasCachedParams;
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

        Vector2 displacement = direction.normalized * cachedParams.speed * Time.deltaTime;
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

        travelEffectTimer += Time.deltaTime;

        // Intentionally left blank for future travel effect logic (e.g., fire trails at tickInterval).
        // Use travelEffectTimer with each effect's tickInterval to decide when to apply.
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
    #endregion
}
