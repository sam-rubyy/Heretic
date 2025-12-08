using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class CameraClampFollow : MonoBehaviour
{
    #region Fields
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    [SerializeField, Min(0f)] private float smoothTime = 0.08f;
    [SerializeField, Min(0f)] private float padding = 0.5f;
    [SerializeField] private bool autoFindPlayer = true;
    [SerializeField] private bool clampToActiveRoom = true;
    [SerializeField, Tooltip("If the target has a Rigidbody2D, use its interpolated position to reduce jitter against walls.")]
    private bool preferRigidbodyPosition = true;
    [SerializeField, Tooltip("If true, snap directly to the clamped position instead of smoothing when at a boundary to avoid jitter.")]
    private bool snapWhenClamped = true;

    private Camera cam;
    private Vector3 velocity;
    private readonly List<Collider2D> colliderBuffer = new List<Collider2D>(16);
    private Rigidbody2D targetBody;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        cam = GetComponent<Camera>();

        if (target == null && autoFindPlayer)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }

        CacheTargetBody();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        CacheTargetBody();
        Vector3 targetPos = target.position;
        if (preferRigidbodyPosition && targetBody != null)
        {
            // Rigidbody2D interpolation gives a smoother visual position when colliding.
            targetPos = targetBody.position;
        }

        Vector3 desired = targetPos + offset;
        bool clamped = false;
        if (clampToActiveRoom && TryGetActiveRoomBounds(out var bounds))
        {
            desired = ClampToBounds(desired, bounds, out clamped);
        }

        // Avoid oscillation at the edges by clearing smoothing velocity when we hit a clamp.
        if (clamped)
        {
            velocity = Vector3.zero;
        }

        transform.position = clamped && snapWhenClamped
            ? desired
            : smoothTime > 0f
            ? Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime)
            : desired;
    }
    #endregion

    #region Private Methods
    private void CacheTargetBody()
    {
        if (!preferRigidbodyPosition || target == null)
        {
            targetBody = null;
            return;
        }

        target.TryGetComponent(out targetBody);
    }

    private bool TryGetActiveRoomBounds(out Bounds bounds)
    {
        bounds = default;

        var manager = RoomManager.Instance;
        if (manager == null || manager.CurrentRoom == null)
        {
            return false;
        }

        colliderBuffer.Clear();
        manager.CurrentRoom.GetComponentsInChildren(true, colliderBuffer);

        bool hasBounds = false;
        for (int i = 0; i < colliderBuffer.Count; i++)
        {
            var col = colliderBuffer[i];
            if (col == null || col.isTrigger)
            {
                continue;
            }

            if (!hasBounds)
            {
                bounds = col.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(col.bounds);
            }
        }

        return hasBounds;
    }

    private Vector3 ClampToBounds(Vector3 desired, Bounds bounds, out bool clamped)
    {
        clamped = false;
        if (!cam.orthographic)
        {
            return desired; // Only orthographic clamping is supported currently.
        }

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        float minX = bounds.min.x + halfWidth + padding;
        float maxX = bounds.max.x - halfWidth - padding;
        float minY = bounds.min.y + halfHeight + padding;
        float maxY = bounds.max.y - halfHeight - padding;

        // If the camera is larger than the bounds, center it.
        if (minX > maxX)
        {
            desired.x = bounds.center.x;
        }
        else
        {
            float originalX = desired.x;
            desired.x = Mathf.Clamp(desired.x, minX, maxX);
            clamped |= !Mathf.Approximately(originalX, desired.x);
        }

        if (minY > maxY)
        {
            desired.y = bounds.center.y;
        }
        else
        {
            float originalY = desired.y;
            desired.y = Mathf.Clamp(desired.y, minY, maxY);
            clamped |= !Mathf.Approximately(originalY, desired.y);
        }

        return desired;
    }
    #endregion
}
