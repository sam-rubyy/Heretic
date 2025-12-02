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
    [SerializeField] private Collider2D triggerCollider;
    [SerializeField] private Transform entryPoint;
    [SerializeField] private Room ownerRoom;
    [SerializeField] private RoomDoor linkedDoor;
    [SerializeField] private bool locked;
    #endregion

    #region Properties
    public DoorDirection Direction => direction;
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

        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
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
    }

    public void SetLocked(bool isLocked)
    {
        locked = isLocked;
        if (triggerCollider != null)
        {
            triggerCollider.enabled = !locked;
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
