using System.Collections.Generic;
using UnityEngine;

public class GeneratedFloorLayout
{
    public readonly Dictionary<Vector2Int, RoomTemplate> Rooms = new Dictionary<Vector2Int, RoomTemplate>();
    public readonly Dictionary<Vector2Int, int> DepthByPosition = new Dictionary<Vector2Int, int>();
    public readonly Dictionary<Vector2Int, RoomTemplate.DoorLayout> DoorMasks = new Dictionary<Vector2Int, RoomTemplate.DoorLayout>();
    public Vector2Int StartPosition { get; set; } = Vector2Int.zero;
    public Vector2Int BossPosition { get; set; } = Vector2Int.zero;

    public RoomTemplate GetTemplate(Vector2Int position)
    {
        Rooms.TryGetValue(position, out var template);
        return template;
    }

    public int GetDepth(Vector2Int position)
    {
        if (DepthByPosition.TryGetValue(position, out int depth))
        {
            return depth;
        }

        return 0;
    }

    public RoomTemplate.DoorLayout GetDoorMask(Vector2Int position)
    {
        DoorMasks.TryGetValue(position, out var mask);
        return mask;
    }

    public int GetDoorCount(Vector2Int position, DoorDirection direction)
    {
        DoorMasks.TryGetValue(position, out var mask);
        return mask.GetDoorCount(direction);
    }
}

public static class FloorLayoutGenerator
{
    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.down,
        Vector2Int.left
    };

    public static GeneratedFloorLayout Generate(FloorConfig config, int seed)
    {
        if (config.ForceStartToTreasureLayout)
        {
            return GenerateDebugStartToTreasureLayout(config);
        }

        var rng = new System.Random(seed);
        var layout = new GeneratedFloorLayout();

        List<Vector2Int> mainPath = BuildMainPath(config, rng);
        for (int i = 0; i < mainPath.Count; i++)
        {
            layout.DepthByPosition[mainPath[i]] = i;
        }

        AddBranches(config, rng, layout, mainPath);

        layout.StartPosition = Vector2Int.zero;
        layout.BossPosition = mainPath[mainPath.Count - 1];
        var forcedRoomTypes = AssignRequiredSpecialRooms(config, layout, rng);

        foreach (var kvp in layout.DepthByPosition)
        {
            Vector2Int position = kvp.Key;
            int depth = kvp.Value;
            var requiredDoors = GetRequiredDoors(layout, position);

            RoomTemplate template = SelectTemplate(config, layout.BossPosition, position, depth, requiredDoors, forcedRoomTypes);
            if (template == null)
            {
                Debug.LogWarning($"No template matched at {position}; using start room as fallback.");
                template = config.StartRoom;
            }

            if (template != null)
            {
                layout.Rooms[position] = template;
            }
        }

        AssignDoorMasks(layout);
        return layout;
    }

    private static GeneratedFloorLayout GenerateDebugStartToTreasureLayout(FloorConfig config)
    {
        var layout = new GeneratedFloorLayout();
        Vector2Int start = Vector2Int.zero;
        Vector2Int treasure = Vector2Int.up;

        layout.StartPosition = start;
        layout.BossPosition = treasure;

        layout.DepthByPosition[start] = 0;
        layout.DepthByPosition[treasure] = 1;

        foreach (var kvp in layout.DepthByPosition)
        {
            Vector2Int position = kvp.Key;
            int depth = kvp.Value;
            var requiredDoors = GetRequiredDoors(layout, position);

            RoomTemplate template = position == start
                ? config.StartRoom
                : config.GetTreasureTemplate(requiredDoors, depth);

            if (template == null)
            {
                Debug.LogWarning($"No template matched at {position}; using start room as fallback.");
                template = config.StartRoom;
            }

            layout.Rooms[position] = template;
        }

        AssignDoorMasks(layout);
        return layout;
    }

    private static List<Vector2Int> BuildMainPath(FloorConfig config, System.Random rng)
    {
        var path = new List<Vector2Int> { Vector2Int.zero };
        var visited = new HashSet<Vector2Int> { Vector2Int.zero };

        int minLength = Mathf.Max(3, config.MainPathLength.x);
        int maxLength = Mathf.Max(minLength, config.MainPathLength.y);
        int targetLength = rng.Next(minLength, maxLength + 1);

        Vector2Int current = Vector2Int.zero;
        for (int i = 0; i < targetLength; i++)
        {
            bool added = false;
            for (int attempt = 0; attempt < 12 && !added; attempt++)
            {
                var direction = Directions[rng.Next(Directions.Length)];
                Vector2Int next = current + direction;
                if (visited.Contains(next))
                {
                    continue;
                }

                path.Add(next);
                visited.Add(next);
                current = next;
                added = true;
            }

            if (!added)
            {
                break;
            }
        }

        return path;
    }

    private static void AddBranches(FloorConfig config, System.Random rng, GeneratedFloorLayout layout, List<Vector2Int> mainPath)
    {
        int branches = config.MaxBranches;
        for (int i = 0; i < branches; i++)
        {
            if (mainPath.Count <= 2)
            {
                break;
            }

            int anchorIndex = rng.Next(1, mainPath.Count - 1);
            Vector2Int anchor = mainPath[anchorIndex];

            var openDirections = new List<Vector2Int>();
            for (int d = 0; d < Directions.Length; d++)
            {
                Vector2Int candidate = anchor + Directions[d];
                if (layout.DepthByPosition.ContainsKey(candidate))
                {
                    continue;
                }

                openDirections.Add(Directions[d]);
            }

            if (openDirections.Count == 0)
            {
                continue;
            }

            Vector2Int dir = openDirections[rng.Next(openDirections.Count)];
            Vector2Int current = anchor + dir;

            for (int segment = 0; segment < config.BranchLength; segment++)
            {
                if (layout.DepthByPosition.ContainsKey(current))
                {
                    break;
                }

                int depth = layout.GetDepth(anchor) + segment + 1;
                layout.DepthByPosition[current] = depth;
                current += dir;
            }
        }
    }

    private static Dictionary<Vector2Int, RoomType> AssignRequiredSpecialRooms(FloorConfig config, GeneratedFloorLayout layout, System.Random rng)
    {
        var forced = new Dictionary<Vector2Int, RoomType>();
        var candidates = new List<Vector2Int>();
        bool hasTreasure = config.HasRoomType(RoomType.Treasure);
        bool hasShop = config.HasRoomType(RoomType.Shop);

        foreach (var kvp in layout.DepthByPosition)
        {
            Vector2Int position = kvp.Key;
            if (position == layout.StartPosition || position == layout.BossPosition)
            {
                continue;
            }

            candidates.Add(position);
        }

        Shuffle(candidates, rng);

        if (hasTreasure && candidates.Count > 0)
        {
            forced[candidates[0]] = RoomType.Treasure;
        }
        else if (hasTreasure)
        {
            Debug.LogWarning("Could not place treasure room; not enough available positions.");
        }

        if (hasShop && candidates.Count > (hasTreasure ? 1 : 0))
        {
            int shopIndex = hasTreasure ? 1 : 0;
            forced[candidates[shopIndex]] = RoomType.Shop;
        }
        else if (hasShop)
        {
            Debug.LogWarning("Could not place shop room; not enough available positions.");
        }

        return forced;
    }

    private static RoomTemplate SelectTemplate(FloorConfig config, Vector2Int bossPosition, Vector2Int position, int depth, RoomTemplate.DoorLayout requiredDoors, Dictionary<Vector2Int, RoomType> forcedRoomTypes)
    {
        if (position == Vector2Int.zero)
        {
            return config.StartRoom;
        }

        if (config.BossRoom != null && position == bossPosition)
        {
            return config.BossRoom;
        }

        if (forcedRoomTypes != null && forcedRoomTypes.TryGetValue(position, out var forcedType))
        {
            RoomTemplate forcedTemplate = null;
            switch (forcedType)
            {
                case RoomType.Treasure:
                    forcedTemplate = config.GetTreasureTemplate(requiredDoors, depth);
                    break;
                case RoomType.Shop:
                    forcedTemplate = config.GetShopTemplate(requiredDoors, depth);
                    break;
            }

            if (forcedTemplate != null)
            {
                return forcedTemplate;
            }

            Debug.LogWarning($"No {forcedType} template matched at {position}; falling back to normal template.");
        }

        RoomTemplate template = config.GetRandomNormalTemplate(requiredDoors, depth);
        if (template == null)
        {
            template = config.GetRandomSpecialTemplate(requiredDoors, depth);
        }

        return template;
    }

    private static RoomTemplate.DoorLayout GetRequiredDoors(GeneratedFloorLayout layout, Vector2Int position)
    {
        return new RoomTemplate.DoorLayout
        {
            north = layout.DepthByPosition.ContainsKey(position + Vector2Int.up),
            east = layout.DepthByPosition.ContainsKey(position + Vector2Int.right),
            south = layout.DepthByPosition.ContainsKey(position + Vector2Int.down),
            west = layout.DepthByPosition.ContainsKey(position + Vector2Int.left)
        };
    }

    private static void AssignDoorMasks(GeneratedFloorLayout layout)
    {
        foreach (var kvp in layout.Rooms)
        {
            Vector2Int position = kvp.Key;
            RoomTemplate template = kvp.Value;
            var mask = new RoomTemplate.DoorLayout();

            for (int i = 0; i < Directions.Length; i++)
            {
                DoorDirection direction = (DoorDirection)i;
                Vector2Int neighborPos = position + Directions[i];

                layout.Rooms.TryGetValue(neighborPos, out var neighborTemplate);

                int availableHere = template != null ? template.Doors.GetDoorCount(direction) : 0;
                int availableNeighbor = neighborTemplate != null ? neighborTemplate.Doors.GetDoorCount(direction.Opposite()) : 0;
                int pairCount = Mathf.Min(availableHere, availableNeighbor);

                SetMaskCount(ref mask, direction, pairCount);
            }

            layout.DoorMasks[position] = mask;
        }
    }

    private static void SetMaskCount(ref RoomTemplate.DoorLayout mask, DoorDirection direction, int count)
    {
        bool primary = count >= 1;
        bool secondary = count >= 2;

        switch (direction)
        {
            case DoorDirection.North:
                mask.north = primary;
                mask.northSecondary = secondary;
                break;
            case DoorDirection.East:
                mask.east = primary;
                mask.eastSecondary = secondary;
                break;
            case DoorDirection.South:
                mask.south = primary;
                mask.southSecondary = secondary;
                break;
            case DoorDirection.West:
                mask.west = primary;
                mask.westSecondary = secondary;
                break;
        }
    }

    private static void Shuffle<T>(List<T> list, System.Random rng)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int swapIndex = rng.Next(i + 1);
            T temp = list[i];
            list[i] = list[swapIndex];
            list[swapIndex] = temp;
        }
    }
}
