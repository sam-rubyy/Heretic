using UnityEngine;

[DisallowMultipleComponent]
public class RoomLootDropTrigger : MonoBehaviour
{
    #region Types
    private enum SpawnTrigger
    {
        OnStart,
        OnEnable,
        OnRoomCleared,
        Manual
    }
    #endregion

    #region Fields
    [SerializeField] private EnemyLootDropper dropper;
    [SerializeField] private SpawnTrigger trigger = SpawnTrigger.OnStart;
    [SerializeField] private bool dropOnlyOnce = true;
    [SerializeField] private RoomType[] allowedRoomTypes = new[] { RoomType.Start, RoomType.Treasure, RoomType.Shop, RoomType.Boss };
    private Room owningRoom;
    private bool hasDropped;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        if (dropper == null)
        {
            dropper = GetComponent<EnemyLootDropper>();
        }

        if (owningRoom == null)
        {
            owningRoom = GetComponentInParent<Room>();
        }
    }

    private void OnEnable()
    {
        if (trigger == SpawnTrigger.OnEnable)
        {
            TrySpawn();
        }

        if (trigger == SpawnTrigger.OnRoomCleared)
        {
            GameplayEvents.OnRoomCleared += HandleRoomCleared;
        }
    }

    private void Start()
    {
        if (trigger == SpawnTrigger.OnStart)
        {
            TrySpawn();
        }
    }

    private void OnDisable()
    {
        if (trigger == SpawnTrigger.OnRoomCleared)
        {
            GameplayEvents.OnRoomCleared -= HandleRoomCleared;
        }
    }
    #endregion

    #region Public Methods
    public void SpawnLoot()
    {
        TrySpawn();
    }
    #endregion

    #region Private Methods
    private void HandleRoomCleared(Room room)
    {
        if (room == owningRoom)
        {
            TrySpawn();
        }
    }

    private void TrySpawn()
    {
        if (dropOnlyOnce && hasDropped)
        {
            return;
        }

        if (!IsRoomAllowed())
        {
            return;
        }

        if (dropper == null)
        {
            return;
        }

        dropper.DropLoot();
        hasDropped = true;
    }

    private bool IsRoomAllowed()
    {
        if (allowedRoomTypes == null || allowedRoomTypes.Length == 0)
        {
            return true;
        }

        RoomType roomType = owningRoom != null && owningRoom.Template != null
            ? owningRoom.Template.RoomType
            : RoomType.Normal;

        for (int i = 0; i < allowedRoomTypes.Length; i++)
        {
            if (allowedRoomTypes[i] == roomType)
            {
                return true;
            }
        }

        return false;
    }
    #endregion
}
