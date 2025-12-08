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
            DeactivateAllDoors(roomInstance);
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
        var doorGroupsCache = new Dictionary<int, Dictionary<Vector2Int, List<RoomDoor>>>();
        var processedPairs = new HashSet<string>();

        foreach (var kvp in generatedRooms)
        {
            Vector2Int position = kvp.Key;
            Room room = kvp.Value;
            if (room == null)
            {
                continue;
            }

            var groups = GetDoorGroups(room, doorGroupsCache);
            foreach (var group in groups)
            {
                Vector2Int offset = group.Key;
                if (offset == Vector2Int.zero)
                {
                    DeactivateDoorGroup(group.Value);
                    continue;
                }

                Vector2Int neighborPos = position + offset;
                if (!generatedRooms.TryGetValue(neighborPos, out var neighborRoom) || neighborRoom == null)
                {
                    DeactivateDoorGroup(group.Value);
                    continue;
                }

                string pairKey = GetPairKey(position, neighborPos);
                if (processedPairs.Contains(pairKey))
                {
                    continue;
                }

                processedPairs.Add(pairKey);
                var neighborGroups = GetDoorGroups(neighborRoom, doorGroupsCache);
                Vector2Int reverseOffset = -offset;

                neighborGroups.TryGetValue(reverseOffset, out var neighborDoors);
                ConnectDoorSets(group.Value, neighborDoors, offset);
            }
        }
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

    #region Door Utilities
    private static void DeactivateAllDoors(Room room)
    {
        var doors = room.GetComponentsInChildren<RoomDoor>(true);
        for (int i = 0; i < doors.Length; i++)
        {
            DeactivateDoor(doors[i]);
        }
    }

    private static Dictionary<Vector2Int, List<RoomDoor>> GetDoorGroups(Room room, Dictionary<int, Dictionary<Vector2Int, List<RoomDoor>>> cache)
    {
        int id = room != null ? room.GetInstanceID() : 0;
        if (cache.TryGetValue(id, out var cached))
        {
            return cached;
        }

        var groups = new Dictionary<Vector2Int, List<RoomDoor>>();
        var doors = room.GetComponentsInChildren<RoomDoor>(true);
        for (int i = 0; i < doors.Length; i++)
        {
            var door = doors[i];
            if (door == null)
            {
                continue;
            }

            Vector2Int offset = door.TargetOffset;
            if (!groups.TryGetValue(offset, out var list))
            {
                list = new List<RoomDoor>();
                groups[offset] = list;
            }

            list.Add(door);
        }

        cache[id] = groups;
        return groups;
    }

    private static void ConnectDoorSets(List<RoomDoor> sourceDoors, List<RoomDoor> targetDoors, Vector2Int offset)
    {
        if (sourceDoors == null || sourceDoors.Count == 0)
        {
            return;
        }

        if (targetDoors == null || targetDoors.Count == 0)
        {
            DeactivateDoorGroup(sourceDoors);
            return;
        }

        SortDoorsForPairing(sourceDoors, offset);
        SortDoorsForPairing(targetDoors, -offset);

        int pairCount = Mathf.Min(sourceDoors.Count, targetDoors.Count);
        for (int i = 0; i < pairCount; i++)
        {
            var source = sourceDoors[i];
            var target = targetDoors[i];
            if (source == null || target == null)
            {
                continue;
            }

            ActivateDoor(source, true);
            ActivateDoor(target, true);
            source.Connect(target);
            target.Connect(source);
        }

        if (sourceDoors.Count > pairCount)
        {
            for (int i = pairCount; i < sourceDoors.Count; i++)
            {
                DeactivateDoor(sourceDoors[i]);
            }
        }

        if (targetDoors.Count > pairCount)
        {
            for (int i = pairCount; i < targetDoors.Count; i++)
            {
                DeactivateDoor(targetDoors[i]);
            }
        }
    }

    private static void SortDoorsForPairing(List<RoomDoor> doors, Vector2Int offset)
    {
        if (doors == null || doors.Count <= 1)
        {
            return;
        }

        bool sortByY = Mathf.Abs(offset.x) >= Mathf.Abs(offset.y);

        doors.Sort((a, b) =>
        {
            if (a == null || b == null)
            {
                return 0;
            }

            float aValue = sortByY ? a.transform.position.y : a.transform.position.x;
            float bValue = sortByY ? b.transform.position.y : b.transform.position.x;
            return aValue.CompareTo(bValue);
        });
    }

    private static void DeactivateDoorGroup(List<RoomDoor> doors)
    {
        if (doors == null)
        {
            return;
        }

        for (int i = 0; i < doors.Count; i++)
        {
            DeactivateDoor(doors[i]);
        }
    }

    private static string GetPairKey(Vector2Int a, Vector2Int b)
    {
        bool aIsLower = a.x < b.x || (a.x == b.x && a.y <= b.y);
        Vector2Int low = aIsLower ? a : b;
        Vector2Int high = aIsLower ? b : a;
        Vector2Int delta = high - low;
        return $"{low.x},{low.y}-{high.x},{high.y}-{delta.x},{delta.y}";
    }

    private static void ActivateDoor(RoomDoor door, bool locked)
    {
        if (door == null)
        {
            return;
        }

        door.SetVisualsEnabled(true);
        door.SetLocked(locked);
    }

    private static void DeactivateDoor(RoomDoor door)
    {
        if (door == null)
        {
            return;
        }

        door.SetLocked(true);
        door.SetVisualsEnabled(false);
    }
    #endregion
}
