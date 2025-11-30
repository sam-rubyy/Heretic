using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Bullet))]
public class BulletController : MonoBehaviour
{
    #region Fields
    [SerializeField] private Bullet bullet;
    private float lifetimeTimer;
    private float distanceTraveled;
    private float travelEffectTimer;
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
        if (bullet == null)
        {
            return;
        }

        BulletParams bulletParams = bullet.GetParameters();
        Vector2 direction = bullet.GetMoveDirection();

        if (direction.sqrMagnitude < 0.001f)
        {
            direction = transform.right;
        }

        Vector2 displacement = direction.normalized * bulletParams.speed * Time.deltaTime;
        transform.position += (Vector3)displacement;
        distanceTraveled += displacement.magnitude;
    }

    private void HandleCollision(Collider2D other)
    {
        if (bullet == null)
        {
            return;
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

        if (bullet == null)
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

            BulletParams bulletParams = bullet.GetParameters();
            Vector2 sourcePos = transform.position;
            playerHealth.TakeDamage(Mathf.RoundToInt(bulletParams.damage), sourcePos, bulletParams.knockback);
            return true;
        }

        var enemyHealth = other.GetComponentInParent<EnemyHealth>();
        if (enemyHealth != null)
        {
            if (owner == BulletOwner.Enemy)
            {
                return false; // Ignore enemy bullets hitting enemies.
            }

            BulletParams bulletParams = bullet.GetParameters();
            Vector2 sourcePos = transform.position;

            enemyHealth.TakeDamage(bulletParams.damage);
            enemyHealth.ApplyKnockback(sourcePos, bulletParams.knockback);

            if (bulletParams.knockback > 0f && other.attachedRigidbody != null && !other.TryGetComponent<IKnockbackReceiver>(out _))
            {
                Vector2 direction = bullet.GetMoveDirection();
                if (direction.sqrMagnitude < 0.001f)
                {
                    direction = transform.right;
                }

                other.attachedRigidbody.AddForce(direction.normalized * bulletParams.knockback, ForceMode2D.Impulse);
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
        if (bullet == null)
        {
            return;
        }

        float maxDistance = bullet.GetParameters().lifetime;
        if (maxDistance <= 0f)
        {
            return;
        }

        lifetimeTimer += Time.deltaTime;
        if (distanceTraveled >= maxDistance)
        {
            Destroy(gameObject);
        }
    }
    #endregion
}
