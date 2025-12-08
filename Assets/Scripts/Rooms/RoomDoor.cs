using UnityEngine;

public enum DoorDirection
{
    North,
    East,
    South,
    West
}

[DisallowMultipleComponent]
public class RoomDoor : MonoBehaviour
{
    #region Fields
    [SerializeField] private DoorDirection direction;
    [SerializeField, Tooltip("Optional override for which grid offset this door connects to. Leave at (0,0) to use the direction's default cardinal offset.")]
    private Vector2Int customTargetOffset = Vector2Int.zero;
    [SerializeField] private Collider2D triggerCollider;
    [SerializeField, Tooltip("Solid collider that blocks the opening when the door is locked or unlinked.")]
    private Collider2D blockCollider;
    [SerializeField, Tooltip("Renderers used to show the door visuals.")]
    private SpriteRenderer[] spriteRenderers;
    [SerializeField] private Transform entryPoint;
    [SerializeField] private Room ownerRoom;
    [SerializeField] private RoomDoor linkedDoor;
    [SerializeField] private bool locked;
    #endregion

    #region Properties
    public DoorDirection Direction => direction;
    public Vector2Int TargetOffset => customTargetOffset != Vector2Int.zero ? customTargetOffset : direction.ToVector();
    public Room OwnerRoom => ownerRoom;
    public RoomDoor LinkedDoor => linkedDoor;
    public bool IsLocked => locked;
    public Vector3 EntryPosition => entryPoint != null ? entryPoint.position : transform.position;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        if (ownerRoom == null)
        {
            ownerRoom = GetComponentInParent<Room>();
        }

        if (triggerCollider == null)
        {
            triggerCollider = GetComponent<Collider2D>();
        }

        if (blockCollider == null)
        {
            var colliders = GetComponentsInChildren<Collider2D>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                var col = colliders[i];
                if (col != null && col != triggerCollider && !col.isTrigger)
                {
                    blockCollider = col;
                    break;
                }
            }
        }

        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        }

        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }

        // Ensure colliders reflect the serialized locked state at startup.
        SetLocked(locked);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (locked || linkedDoor == null)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            RoomManager.Instance?.EnterConnectedRoom(this, linkedDoor);
        }
    }
    #endregion

    #region Public Methods
    public void Connect(RoomDoor other)
    {
        linkedDoor = other;
        // Refresh collider state now that linkage is known.
        SetLocked(locked);
    }

    public void SetLocked(bool isLocked)
    {
        locked = isLocked;
        if (triggerCollider != null)
        {
            triggerCollider.enabled = !locked && linkedDoor != null;
        }

        if (blockCollider != null)
        {
            blockCollider.enabled = locked || linkedDoor == null;
        }
    }

    public void SetVisualsEnabled(bool enabled)
    {
        if (spriteRenderers == null)
        {
            return;
        }

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            var sr = spriteRenderers[i];
            if (sr != null)
            {
                sr.enabled = enabled;
            }
        }
    }
    #endregion
}

public static class DoorDirectionExtensions
{
    public static DoorDirection Opposite(this DoorDirection direction)
    {
        switch (direction)
        {
            case DoorDirection.North: return DoorDirection.South;
            case DoorDirection.East: return DoorDirection.West;
            case DoorDirection.South: return DoorDirection.North;
            case DoorDirection.West: return DoorDirection.East;
            default: return DoorDirection.North;
        }
    }

    public static Vector2Int ToVector(this DoorDirection direction)
    {
        switch (direction)
        {
            case DoorDirection.North: return Vector2Int.up;
            case DoorDirection.East: return Vector2Int.right;
            case DoorDirection.South: return Vector2Int.down;
            case DoorDirection.West: return Vector2Int.left;
            default: return Vector2Int.zero;
        }
    }
}
