using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Heretic/Rooms/Room Template")]
public class RoomTemplate : ScriptableObject
{
    #region Types
    [Serializable]
    public struct DoorLayout
    {
        [Tooltip("Door on the +Y side of the room.")]
        public bool north;
        [Tooltip("Door on the +X side of the room.")]
        public bool east;
        [Tooltip("Door on the -Y side of the room.")]
        public bool south;
        [Tooltip("Door on the -X side of the room.")]
        public bool west;

        public bool HasDirection(DoorDirection direction)
        {
            switch (direction)
            {
                case DoorDirection.North: return north;
                case DoorDirection.East: return east;
                case DoorDirection.South: return south;
                case DoorDirection.West: return west;
                default: return false;
            }
        }

        public bool MatchesRequiredDoors(DoorLayout required)
        {
            if (required.north && !north) return false;
            if (required.east && !east) return false;
            if (required.south && !south) return false;
            if (required.west && !west) return false;
            return true;
        }
    }
    #endregion

    #region Fields
    [SerializeField] private Room roomPrefab;
    [SerializeField] private RoomType roomType = RoomType.Normal;
    [SerializeField] private DoorLayout doors;
    [SerializeField, Min(1)] private int weight = 1;
    [SerializeField, Min(0)] private int difficultyRating = 1;
    [SerializeField, Tooltip("Inclusive min depth this template can appear on the main path.")]
    private int minDepth = 0;
    [SerializeField, Tooltip("Inclusive max depth this template can appear on the main path.")]
    private int maxDepth = 20;
    #endregion

    #region Properties
    public Room RoomPrefab => roomPrefab;
    public RoomType RoomType => roomType;
    public DoorLayout Doors => doors;
    public int Weight => Mathf.Max(1, weight);
    public int DifficultyRating => Mathf.Max(0, difficultyRating);
    public int MinDepth => minDepth;
    public int MaxDepth => maxDepth;
    #endregion

    #region Public Methods
    public bool SupportsDoors(RoomTemplate.DoorLayout requiredDoors)
    {
        return doors.MatchesRequiredDoors(requiredDoors);
    }
    #endregion
}

public enum RoomType
{
    Start,
    Normal,
    Treasure,
    Shop,
    Boss
}
