using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(EnemyLootDropper))]
public class TestEnemy : EnemyBase
    , IKnockbackReceiver
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float chaseRange = 8f;
    [SerializeField] private Transform target;
    [Header("Contact Damage")]
    [SerializeField] private int contactDamage = 1;
    [SerializeField] private float contactKnockback = 4f;
    [SerializeField] private float contactCooldown = 0.5f;
    [Header("Pathfinding")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float pathCellSize = 0.5f;
    [SerializeField] private float repathInterval = 0.35f;
    [SerializeField] private float waypointTolerance = 0.05f;
    [SerializeField] private float stopDistance = 0.1f;
    [SerializeField] private float pathClearancePadding = 0.05f;
    [Header("Abilities")]
    [SerializeField] private AbilityController abilityController;

    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    private float lastContactTime;
    private float lastPathTime;
    private List<Vector2> currentPath = new List<Vector2>();
    private int currentWaypoint;
    private Vector2 knockbackVelocity;
    [SerializeField] private float knockbackDamping = 12f;
    private float agentClearance;

    protected override void Awake()
    {
        base.Awake();
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        agentClearance = (col != null ? Mathf.Max(col.bounds.extents.x, col.bounds.extents.y) : 0f) + Mathf.Max(0f, pathClearancePadding);
        if (abilityController == null)
        {
            abilityController = GetComponent<AbilityController>();
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

    public override void Initialize()
    {
        base.Initialize();
        EnsureTarget();
        if (abilityController != null)
        {
            abilityController.SetTarget(target);
            abilityController.ResetCooldowns();
        }
    }

    public override void OnSpawned()
    {
        base.OnSpawned();
        EnsureTarget();
        if (abilityController != null)
        {
            abilityController.SetTarget(target);
        }
    }

    private void FixedUpdate()
    {
        if (target == null || body == null)
        {
            return;
        }

        Vector2 toTarget = (Vector2)(target.position - transform.position);
        float distance = toTarget.magnitude;

        if (distance <= 0.01f || distance > chaseRange)
        {
            body.velocity = Vector2.zero;
            return;
        }

        // Apply knockback first; while active, skip pathing.
        if (knockbackVelocity.sqrMagnitude > 0.0001f)
        {
            Vector2 kbDelta = knockbackVelocity * Time.fixedDeltaTime;
            body.MovePosition(body.position + kbDelta);
            knockbackVelocity = Vector2.MoveTowards(knockbackVelocity, Vector2.zero, knockbackDamping * Time.fixedDeltaTime);
            return;
        }

        UpdatePath();
        Vector2 moveDir = GetMoveDirection(distance, toTarget);
        Vector2 nextPos = body.position + moveDir * moveSpeed * Time.fixedDeltaTime;

        body.MovePosition(nextPos);
        body.velocity = moveDir * moveSpeed;

        if (spriteRenderer != null && Mathf.Abs(moveDir.x) > 0.01f)
        {
            spriteRenderer.flipX = moveDir.x < 0f;
        }
    }

    private void EnsureTarget()
    {
        if (target != null)
        {
            return;
        }

        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            target = player.transform;
            if (abilityController != null)
            {
                abilityController.SetTarget(target);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        if (currentPath != null && currentPath.Count > 0)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < currentPath.Count; i++)
            {
                Gizmos.DrawWireCube(currentPath[i], Vector2.one * pathCellSize * 0.9f);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleContact(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleContact(other);
    }

    private void HandleContact(Collider2D other)
    {
        if (other == null)
        {
            return;
        }

        if (Time.time < lastContactTime + contactCooldown)
        {
            return;
        }

        // Colliders may be on child objects; climb the hierarchy to find PlayerHealth.
        var playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth == null)
        {
            return;
        }

        lastContactTime = Time.time;
        playerHealth.TakeDamage(contactDamage, transform.position, contactKnockback);
    }

    private void UpdatePath()
    {
        if (Time.time < lastPathTime + repathInterval)
        {
            return;
        }

        lastPathTime = Time.time;

        if (target == null)
        {
            currentPath.Clear();
            currentWaypoint = 0;
            return;
        }

        if (AStarPathfinder.TryGetPath(
                body.position,
                target.position,
                pathCellSize,
                obstacleMask,
                agentClearance,
                4096,
                out var path))
        {
            currentPath = path;
            if (currentPath.Count > 0)
            {
                // Drop the start cell so we don't steer back toward our current position on replans.
                currentPath.RemoveAt(0);
            }
            currentWaypoint = 0;
        }
        else
        {
            currentPath.Clear();
            currentWaypoint = 0;
        }
    }

    private Vector2 GetMoveDirection(float distanceToTarget, Vector2 directVector)
    {
        if (currentPath == null || currentPath.Count == 0)
        {
            return directVector.normalized;
        }

        // Skip waypoints we've reached.
        while (currentWaypoint < currentPath.Count &&
               Vector2.Distance(body.position, currentPath[currentWaypoint]) <= waypointTolerance)
        {
            currentWaypoint++;
        }

        // If at end of path, go straight to target.
        if (currentWaypoint >= currentPath.Count)
        {
            return distanceToTarget > stopDistance ? directVector.normalized : Vector2.zero;
        }

        Vector2 waypoint = currentPath[currentWaypoint];
        Vector2 dir = waypoint - body.position;
        if (dir.sqrMagnitude < 0.0001f)
        {
            return Vector2.zero;
        }

        return dir.normalized;
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        if (force <= 0f)
            return;

        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector2.right;

        knockbackVelocity += direction.normalized * force;
    }
}
