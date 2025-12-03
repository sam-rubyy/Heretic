using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class DashRunner : MonoBehaviour
{
    #region Fields
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private float defaultBaseSpeed = 8f;
    [SerializeField] private PlayerMovement playerMovement;
    private Coroutine routine;
    private Vector2 cachedVelocity;
    private float activeDashSpeed;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        if (body == null)
        {
            body = GetComponent<Rigidbody2D>();
        }

        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
        }
    }

    private void OnDisable()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
            RestoreVelocity();
        }
    }
    #endregion

    #region Public Methods
    public void Trigger(Vector2 direction, float speedMultiplier, float durationSeconds, bool useUnscaledTime)
    {
        if (direction.sqrMagnitude < 0.0001f || speedMultiplier <= 0f || durationSeconds <= 0f)
        {
            return;
        }

        if (routine != null)
        {
            StopCoroutine(routine);
        }

        routine = StartCoroutine(DashRoutine(direction.normalized, speedMultiplier, durationSeconds, useUnscaledTime));
    }

    public bool IsDashing => routine != null;
    public float ActiveDashSpeed => activeDashSpeed;
    #endregion

    #region Private Methods
    private IEnumerator DashRoutine(Vector2 direction, float speedMultiplier, float durationSeconds, bool useUnscaledTime)
    {
        Vector2 baseVelocity = CacheVelocity();
        float baseSpeed = Mathf.Max(GetBaseSpeed(), baseVelocity.magnitude);
        float dashSpeed = baseSpeed * Mathf.Max(0f, speedMultiplier);
        activeDashSpeed = dashSpeed;

        float elapsed = 0f;
        while (elapsed < durationSeconds)
        {
            float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            if (body != null)
            {
                body.velocity = direction * dashSpeed;
            }
            else
            {
                Vector2 delta = direction * dashSpeed * deltaTime;
                transform.position += (Vector3)delta;
            }

            elapsed += deltaTime;
            yield return null;
        }

        RestoreVelocity();
        activeDashSpeed = 0f;
        routine = null;
    }

    private Vector2 CacheVelocity()
    {
        if (body == null)
        {
            cachedVelocity = Vector2.zero;
            return cachedVelocity;
        }

        cachedVelocity = body.velocity;
        body.velocity = Vector2.zero;
        return cachedVelocity;
    }

    private float GetBaseSpeed()
    {
        if (playerMovement != null)
        {
            float moveSpeed = playerMovement.GetMoveSpeed();
            if (moveSpeed > 0f)
            {
                return moveSpeed;
            }
        }

        return defaultBaseSpeed;
    }

    private void RestoreVelocity()
    {
        if (body != null)
        {
            body.velocity = cachedVelocity;
        }
        activeDashSpeed = 0f;
    }
    #endregion
}
