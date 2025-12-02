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

    private Camera cam;
    private Vector3 velocity;
    private readonly List<Collider2D> colliderBuffer = new List<Collider2D>(16);
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
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desired = target.position + offset;
        if (clampToActiveRoom && TryGetActiveRoomBounds(out var bounds))
        {
            desired = ClampToBounds(desired, bounds);
        }

        transform.position = smoothTime > 0f
            ? Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime)
            : desired;
    }
    #endregion

    #region Private Methods
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

    private Vector3 ClampToBounds(Vector3 desired, Bounds bounds)
    {
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
            desired.x = Mathf.Clamp(desired.x, minX, maxX);
        }

        if (minY > maxY)
        {
            desired.y = bounds.center.y;
        }
        else
        {
            desired.y = Mathf.Clamp(desired.y, minY, maxY);
        }

        return desired;
    }
    #endregion
}
