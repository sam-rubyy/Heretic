using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class RoomManager : MonoBehaviour
{
    #region Fields
    [SerializeField] private List<Room> rooms = new List<Room>();
    [SerializeField] private Room currentRoom;
    [Header("Generation")]
    [SerializeField] private List<FloorConfig> floorConfigs = new List<FloorConfig>();
    [SerializeField] private int startingFloorIndex = 0;
    [SerializeField] private bool generateOnStart = true;
    [SerializeField, Tooltip("Offset applied per grid cell when spawning generated rooms.")]
    private float roomSpacing = 20f;
    [SerializeField, Tooltip("Use 0 for a random seed each run.")]
    private int generationSeed = 0;
    private readonly Dictionary<Vector2Int, Room> generatedRooms = new Dictionary<Vector2Int, Room>();
    private GeneratedFloorLayout currentLayout;
    private FloorConfig activeFloor;
    private bool usingGeneratedLayout;
    private GameObject cachedPlayer;
    #endregion

    #region Properties
    public static RoomManager Instance { get; private set; }
    public Room CurrentRoom => currentRoom;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (generateOnStart && floorConfigs.Count > 0)
        {
            BuildFloor(startingFloorIndex);
        }
        else if (rooms.Count == 0)
        {
            return;
        }

        if (currentRoom == null && rooms.Count > 0)
        {
            currentRoom = rooms[0];
        }

        SetActiveRoom(currentRoom);
        GameplayEvents.OnRoomCleared += HandleRoomCleared;
        GameplayEvents.OnPlayerDied += HandlePlayerDied;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        GameplayEvents.OnRoomCleared -= HandleRoomCleared;
        GameplayEvents.OnPlayerDied -= HandlePlayerDied;
    }
    #endregion

    #region Public Methods
    public void LoadNextRoom()
    {
        if (usingGeneratedLayout)
        {
            return;
        }

        if (rooms.Count == 0)
        {
            return;
        }

        int currentIndex = rooms.IndexOf(currentRoom);
        if (currentIndex < 0)
        {
            currentIndex = 0;
        }

        int nextIndex = currentIndex + 1;
        if (nextIndex >= rooms.Count)
        {
            RestartRun();
            return;
        }

        SetActiveRoom(rooms[nextIndex]);
    }

    public void RestartRun()
    {
        if (usingGeneratedLayout)
        {
            BuildFloor(startingFloorIndex);
            return;
        }

        if (rooms.Count == 0)
        {
            return;
        }

        SetActiveRoom(rooms[0]);
    }

    public void BuildFloor(int floorIndex)
    {
        if (floorConfigs.Count == 0)
        {
            Debug.LogWarning("No FloorConfigs assigned. Falling back to serialized rooms list.");
            usingGeneratedLayout = false;
            return;
        }

        usingGeneratedLayout = true;
        ClearGeneratedRooms();

        int index = Mathf.Clamp(floorIndex, 0, floorConfigs.Count - 1);
        activeFloor = floorConfigs[index];

        int seedToUse = generationSeed != 0 ? generationSeed : UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        UnityEngine.Random.InitState(seedToUse);
        currentLayout = FloorLayoutGenerator.Generate(activeFloor, seedToUse);

        foreach (var kvp in currentLayout.Rooms)
        {
            Vector2Int gridPosition = kvp.Key;
            RoomTemplate template = kvp.Value;
            if (template == null || template.RoomPrefab == null)
            {
                continue;
            }

            Vector3 worldPos = new Vector3(gridPosition.x * roomSpacing, gridPosition.y * roomSpacing, 0f);
            Room roomInstance = Instantiate(template.RoomPrefab, worldPos, Quaternion.identity, transform);
            roomInstance.Configure(template, activeFloor, currentLayout.GetDepth(gridPosition));
            roomInstance.gameObject.SetActive(false);
            generatedRooms[gridPosition] = roomInstance;
        }

        ConnectGeneratedDoors();

        Room startRoom = GetRoomAt(currentLayout.StartPosition);
        if (startRoom == null)
        {
            Debug.LogWarning("Generated floor missing a start room. Using first generated room instead.");
            foreach (var room in generatedRooms.Values)
            {
                startRoom = room;
                break;
            }
        }

        currentRoom = startRoom;
        SetActiveRoom(currentRoom);
    }

    public void EnterConnectedRoom(RoomDoor fromDoor, RoomDoor toDoor)
    {
        if (toDoor == null)
        {
            return;
        }

        Room targetRoom = toDoor.OwnerRoom;
        if (targetRoom == null || targetRoom == currentRoom)
        {
            return;
        }

        SetActiveRoom(targetRoom);
        MovePlayerTo(toDoor.EntryPosition);
    }
    #endregion

    #region Private Methods
    private void HandleRoomCleared(Room room)
    {
        if (room == currentRoom)
        {
            if (!usingGeneratedLayout)
            {
                LoadNextRoom();
            }
        }

        if (room != null)
        {
            room.UnlockDoors();
        }
    }

    private void HandlePlayerDied(PlayerHealth playerHealth)
    {
        playerHealth?.ResetHealth();
        RestartRun();
        MovePlayerToStartRoom();
    }

    private void MovePlayerToStartRoom()
    {
        if (currentRoom != null)
        {
            MovePlayerTo(currentRoom.transform.position);
        }
    }

    private void SetActiveRoom(Room room)
    {
        currentRoom = room;

        foreach (var targetRoom in GetAllRooms())
        {
            if (targetRoom == null)
            {
                continue;
            }

            bool shouldBeActive = targetRoom == currentRoom;
            if (targetRoom.gameObject.activeSelf != shouldBeActive)
            {
                targetRoom.gameObject.SetActive(shouldBeActive);
            }

            if (shouldBeActive)
            {
                targetRoom.OnActivated();
            }
        }
    }

    private IEnumerable<Room> GetAllRooms()
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            var room = rooms[i];
            if (room != null)
            {
                yield return room;
            }
        }

        foreach (var generated in generatedRooms.Values)
        {
            if (generated != null)
            {
                yield return generated;
            }
        }
    }

    private void ClearGeneratedRooms()
    {
        foreach (var room in generatedRooms.Values)
        {
            if (room != null)
            {
                Destroy(room.gameObject);
            }
        }

        generatedRooms.Clear();
        currentLayout = null;
    }

    private Room GetRoomAt(Vector2Int gridPosition)
    {
        generatedRooms.TryGetValue(gridPosition, out var room);
        return room;
    }

    private void ConnectGeneratedDoors()
    {
        foreach (var kvp in generatedRooms)
        {
            Vector2Int position = kvp.Key;
            Room room = kvp.Value;
            if (room == null)
            {
                continue;
            }

            var doors = room.GetComponentsInChildren<RoomDoor>(true);
            for (int i = 0; i < doors.Length; i++)
            {
                var door = doors[i];
                if (door == null)
                {
                    continue;
                }

                Vector2Int neighborPos = position + door.Direction.ToVector();
                if (!generatedRooms.TryGetValue(neighborPos, out var neighborRoom) || neighborRoom == null)
                {
                    door.SetLocked(true);
                    continue;
                }

                var neighborDoor = FindDoor(neighborRoom, door.Direction.Opposite());
                if (neighborDoor == null)
                {
                    door.SetLocked(true);
                    continue;
                }

                door.Connect(neighborDoor);
                neighborDoor.Connect(door);
            }
        }
    }

    private RoomDoor FindDoor(Room room, DoorDirection direction)
    {
        var doors = room.GetComponentsInChildren<RoomDoor>(true);
        for (int i = 0; i < doors.Length; i++)
        {
            if (doors[i].Direction == direction)
            {
                return doors[i];
            }
        }

        return null;
    }

    private void MovePlayerTo(Vector3 position)
    {
        if (cachedPlayer == null)
        {
            cachedPlayer = GameObject.FindGameObjectWithTag("Player");
        }

        if (cachedPlayer == null)
        {
            return;
        }

        var body = cachedPlayer.GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.position = position;
        }
        else
        {
            cachedPlayer.transform.position = position;
        }
    }
    #endregion
}
