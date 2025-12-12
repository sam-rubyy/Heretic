using UnityEngine;

public enum BulletOwner
{
    Neutral,
    Player,
    Enemy
}

[DisallowMultipleComponent]
public class Bullet : MonoBehaviour
{
    #region Fields
    [SerializeField] private BulletParams bulletParameters;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private Collider2D hitbox;
    private BulletController controller;
    private Vector2 moveDirection = Vector2.right;
    private BulletOwner owner = BulletOwner.Neutral;
    private Color baseColor = Color.white;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        controller = GetComponent<BulletController>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            baseColor = spriteRenderer.color;
        }

        // Ensure trigger-based collision and no physics forces push the bullet around.
        if (body == null)
        {
            body = GetComponent<Rigidbody2D>();
        }

        if (body == null)
        {
            body = gameObject.AddComponent<Rigidbody2D>();
        }

        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.velocity = Vector2.zero;
        body.angularVelocity = 0f;
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        if (hitbox == null)
        {
            hitbox = GetComponent<Collider2D>();
        }

        if (hitbox == null)
        {
            hitbox = gameObject.AddComponent<CircleCollider2D>();
        }

        hitbox.isTrigger = true;
    }
    #endregion

    #region Public Methods
    public void Initialize(BulletParams parameters)
    {
        bulletParameters = parameters;
        ApplyTint();
        ApplyOrientation(transform.right, false);
    }

    public void Initialize(BulletParams parameters, Vector2 direction)
    {
        bulletParameters = parameters;
        if (direction.sqrMagnitude > 0.001f)
        {
            moveDirection = direction.normalized;
        }

        ApplyTint();
        ApplyOrientation(moveDirection, true);
    }

    public StatusEffectParams[] GetOnHitEffects() => bulletParameters.onHitEffects;

    public TravelEffectParams[] GetOnTravelEffects() => bulletParameters.onTravelEffects;

    public BulletParams GetParameters() => bulletParameters;

    public Vector2 GetMoveDirection() => moveDirection;

    public void SetOwner(BulletOwner newOwner) => owner = newOwner;

    public BulletOwner GetOwner() => owner;
    #endregion

    #region Private Methods
    private void ApplyTint()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        Color tint = bulletParameters.tint;
        bool shouldTint = bulletParameters.overrideTint || tint.a > 0.0001f;
        spriteRenderer.color = shouldTint ? tint : baseColor;
    }

    private void ApplyOrientation(Vector2 direction, bool alignTransform)
    {
        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        var normalized = direction.normalized;

        if (alignTransform)
        {
            transform.right = normalized;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.flipY = normalized.x < 0f;
        }
    }
    #endregion
}
