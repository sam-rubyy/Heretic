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

        ApplyDamage(other);
        ProcessOnHitEffects(other);

        Destroy(gameObject);
    }

    private void ApplyDamage(Collider2D other)
    {
        if (!other.TryGetComponent<EnemyHealth>(out var enemyHealth))
        {
            return;
        }

        BulletParams bulletParams = bullet.GetParameters();
        enemyHealth.TakeDamage(bulletParams.damage);

        if (bulletParams.knockback > 0f && other.attachedRigidbody != null)
        {
            Vector2 direction = bullet.GetMoveDirection();
            if (direction.sqrMagnitude < 0.001f)
            {
                direction = transform.right;
            }

            other.attachedRigidbody.AddForce(direction.normalized * bulletParams.knockback, ForceMode2D.Impulse);
        }
    }

    private void ProcessOnHitEffects(Collider2D other)
    {
        var effects = bullet.GetOnHitEffects();

        // Intentionally left blank for future effect application logic (e.g., chance-based burning).
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
