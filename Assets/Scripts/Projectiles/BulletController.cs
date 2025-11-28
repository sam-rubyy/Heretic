using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Bullet))]
public class BulletController : MonoBehaviour
{
    #region Fields
    [SerializeField] private Bullet bullet;
    private float lifetimeTimer;
    private float distanceTraveled;
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
        ProcessOnHitEffects(other);
    }

    private void ProcessOnHitEffects(Collider2D other)
    {
        var effects = bullet.GetOnHitEffects();

        // Intentionally left blank for future effect application logic (e.g., chance-based burning).
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
