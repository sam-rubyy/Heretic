using System.Collections.Generic;
using UnityEngine;

public class GeneratedFloorLayout
{
    public readonly Dictionary<Vector2Int, RoomTemplate> Rooms = new Dictionary<Vector2Int, RoomTemplate>();
    public readonly Dictionary<Vector2Int, int> DepthByPosition = new Dictionary<Vector2Int, int>();
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

        foreach (var kvp in layout.DepthByPosition)
        {
            Vector2Int position = kvp.Key;
            int depth = kvp.Value;
            var requiredDoors = GetRequiredDoors(layout, position);

            RoomTemplate template = SelectTemplate(config, layout.BossPosition, position, depth, requiredDoors);
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

        return layout;
    }

    private static List<Vector2Int> BuildMainPath(FloorConfig config, System.Random rng)
    {
        var path = new List<Vector2Int> { Vector2Int.zero };
        var visited = new HashSet<Vector2Int> { Vector2Int.zero };

        int minLength = Mathf.Max(2, config.MainPathLength.x);
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

    private static RoomTemplate SelectTemplate(FloorConfig config, Vector2Int bossPosition, Vector2Int position, int depth, RoomTemplate.DoorLayout requiredDoors)
    {
        if (position == Vector2Int.zero)
        {
            return config.StartRoom;
        }

        if (config.BossRoom != null && position == bossPosition)
        {
            return config.BossRoom;
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
}
