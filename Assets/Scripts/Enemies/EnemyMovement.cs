using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyHealth))]
public class EnemyMovement : MonoBehaviour, IKnockbackReceiver
{
    #region Fields
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float accelerationTime = 0.08f;
    [SerializeField] private float decelerationTime = 0.12f;
    [SerializeField] private float chaseRange = 12f;
    [SerializeField] private float stopDistance = 0.5f;
    [SerializeField] private float knockbackDamping = 10f;
    [Header("Separation")]
    [SerializeField, Tooltip("Radius to check for nearby enemies to spread out.")] private float separationRadius = 1.25f;
    [SerializeField, Tooltip("How strongly to push away from nearby enemies.")] private float separationStrength = 2f;
    [SerializeField, Tooltip("Layer mask used to find other enemies for separation.")] private LayerMask separationMask = ~0;
    [SerializeField, Tooltip("Max neighbors to consider for separation to limit cost.")] private int maxNeighbors = 8;
    [SerializeField] private Transform target;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Rigidbody2D body;
    private EnemyHealth enemyHealth;
    private Vector2 smoothedVelocity;
    private Vector2 smoothVelocityRef;
    private Vector2 knockbackVelocity;
    private readonly Collider2D[] neighborsBuffer = new Collider2D[16];
    #endregion

    #region Unity Methods
    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        enemyHealth = GetComponent<EnemyHealth>();

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (target == null)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
    }

    private void FixedUpdate()
    {
        float deltaTime = Time.fixedDeltaTime;
        if (deltaTime <= 0f || body == null)
        {
            return;
        }

        Vector2 desiredVelocity = Vector2.zero;
        Vector2 toTarget = Vector2.zero;
        if (target != null)
        {
            toTarget = (Vector2)(target.position - transform.position);
            float distance = toTarget.magnitude;
            if (distance > stopDistance && distance <= chaseRange)
            {
                float speedFactor = enemyHealth != null ? enemyHealth.MoveSpeedMultiplier : 1f;
                desiredVelocity = toTarget.normalized * moveSpeed * speedFactor;
            }
        }

        float smoothTime = desiredVelocity.sqrMagnitude > 0.001f ? accelerationTime : decelerationTime;
        Vector2 separation = separationStrength > 0f && separationRadius > 0.01f
            ? ComputeSeparation()
            : Vector2.zero;

        smoothedVelocity = Vector2.SmoothDamp(smoothedVelocity, desiredVelocity + separation, ref smoothVelocityRef, smoothTime, Mathf.Infinity, deltaTime);

        // Decay knockback over time.
        knockbackVelocity = Vector2.MoveTowards(knockbackVelocity, Vector2.zero, knockbackDamping * deltaTime);

        Vector2 finalVelocity = smoothedVelocity + knockbackVelocity;
        if (body.bodyType == RigidbodyType2D.Dynamic)
        {
            body.velocity = finalVelocity;
        }
        else
        {
            body.MovePosition(body.position + finalVelocity * deltaTime);
        }

        if (spriteRenderer != null && toTarget.sqrMagnitude > 0.001f)
        {
            spriteRenderer.flipX = toTarget.x < 0f;
        }
    }
    #endregion

    #region Public Methods
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        if (force <= 0f)
        {
            return;
        }

        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = Vector2.right;
        }

        Vector2 newVelocity = direction.normalized * force;
        float maxMagnitude = Mathf.Max(force, knockbackVelocity.magnitude);
        knockbackVelocity = Vector2.ClampMagnitude(newVelocity, maxMagnitude);
    }
    #endregion

    #region Private Methods
    private Vector2 ComputeSeparation()
    {
        int count = Physics2D.OverlapCircleNonAlloc(transform.position, separationRadius, neighborsBuffer, separationMask);
        if (count <= 0)
        {
            return Vector2.zero;
        }

        Vector2 push = Vector2.zero;
        int considered = 0;

        for (int i = 0; i < count && considered < maxNeighbors; i++)
        {
            var col = neighborsBuffer[i];
            if (col == null || col.attachedRigidbody == body)
            {
                continue;
            }

            // Only separate from other enemies.
            if (col.GetComponentInParent<EnemyMovement>() == null)
            {
                continue;
            }

            Vector2 away = (Vector2)(transform.position - col.transform.position);
            float sqrDist = away.sqrMagnitude;
            if (sqrDist < 0.0001f)
            {
                continue;
            }

            float dist = Mathf.Sqrt(sqrDist);
            float weight = 1f - Mathf.Clamp01(dist / separationRadius);
            push += away.normalized * weight;
            considered++;
        }

        if (considered == 0)
        {
            return Vector2.zero;
        }

        return push.normalized * separationStrength;
    }
    #endregion
}
