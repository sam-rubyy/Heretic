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
