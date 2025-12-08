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
        [Tooltip("Optional second door on the +Y side (for wider rooms).")]
        public bool northSecondary;
        [Tooltip("Door on the +X side of the room.")]
        public bool east;
        [Tooltip("Optional second door on the +X side (for wider rooms).")]
        public bool eastSecondary;
        [Tooltip("Door on the -Y side of the room.")]
        public bool south;
        [Tooltip("Optional second door on the -Y side (for wider rooms).")]
        public bool southSecondary;
        [Tooltip("Door on the -X side of the room.")]
        public bool west;
        [Tooltip("Optional second door on the -X side (for wider rooms).")]
        public bool westSecondary;

        public bool HasDirection(DoorDirection direction)
        {
            return GetDoorCount(direction) > 0;
        }

        public int GetDoorCount(DoorDirection direction)
        {
            switch (direction)
            {
                case DoorDirection.North: return CountDoors(north, northSecondary);
                case DoorDirection.East: return CountDoors(east, eastSecondary);
                case DoorDirection.South: return CountDoors(south, southSecondary);
                case DoorDirection.West: return CountDoors(west, westSecondary);
                default: return 0;
            }
        }

        public bool MatchesRequiredDoors(DoorLayout required)
        {
            if (GetDoorCount(DoorDirection.North) < required.GetDoorCount(DoorDirection.North)) return false;
            if (GetDoorCount(DoorDirection.East) < required.GetDoorCount(DoorDirection.East)) return false;
            if (GetDoorCount(DoorDirection.South) < required.GetDoorCount(DoorDirection.South)) return false;
            if (GetDoorCount(DoorDirection.West) < required.GetDoorCount(DoorDirection.West)) return false;
            return true;
        }

        private static int CountDoors(bool primary, bool secondary)
        {
            return (primary ? 1 : 0) + (secondary ? 1 : 0);
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
