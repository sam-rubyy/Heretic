using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lightweight grid-based A* pathfinder that samples Physics2D colliders as obstacles.
/// </summary>
public static class AStarPathfinder
{
    private struct Node
    {
        public Vector2Int Coord;
        public float G;
        public float H;
        public Vector2Int Parent;
        public bool HasParent;

        public float F => G + H;
    }

    public static bool TryGetPath(
        Vector2 start,
        Vector2 goal,
        float cellSize,
        LayerMask obstacleMask,
        float clearance,
        int maxIterations,
        out List<Vector2> path)
    {
        path = null;

        if (cellSize <= 0f)
        {
            return false;
        }

        Vector2Int startCoord = WorldToCoord(start, cellSize);
        Vector2Int goalCoord = WorldToCoord(goal, cellSize);

        var open = new List<Vector2Int> { startCoord };
        var nodes = new Dictionary<Vector2Int, Node>();
        nodes[startCoord] = new Node
        {
            Coord = startCoord,
            G = 0f,
            H = Heuristic(startCoord, goalCoord),
            HasParent = false
        };

        var closed = new HashSet<Vector2Int>();
        int iterations = 0;

        while (open.Count > 0 && iterations < maxIterations)
        {
            iterations++;

            Vector2Int current = FindLowestF(open, nodes);
            if (current == goalCoord)
            {
                path = Reconstruct(nodes, current, cellSize);
                return true;
            }

            open.Remove(current);
            closed.Add(current);

            foreach (Vector2Int neighbor in GetNeighbors(current))
            {
                if (closed.Contains(neighbor))
                {
                    continue;
                }

                Vector2 neighborWorld = CoordToWorld(neighbor, cellSize);
                if (IsBlocked(neighborWorld, cellSize, clearance, obstacleMask))
                {
                    continue;
                }

                float tentativeG = nodes[current].G + cellSize;
                bool inOpen = nodes.ContainsKey(neighbor) && open.Contains(neighbor);

                if (!inOpen || tentativeG < nodes[neighbor].G)
                {
                    nodes[neighbor] = new Node
                    {
                        Coord = neighbor,
                        G = tentativeG,
                        H = Heuristic(neighbor, goalCoord),
                        Parent = current,
                        HasParent = true
                    };

                    if (!inOpen)
                    {
                        open.Add(neighbor);
                    }
                }
            }
        }

        return false;
    }

    private static Vector2Int WorldToCoord(Vector2 worldPos, float cellSize)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPos.x / cellSize),
            Mathf.RoundToInt(worldPos.y / cellSize));
    }

    private static Vector2 CoordToWorld(Vector2Int coord, float cellSize)
    {
        return new Vector2(coord.x * cellSize, coord.y * cellSize);
    }

    private static float Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private static IEnumerable<Vector2Int> GetNeighbors(Vector2Int coord)
    {
        yield return new Vector2Int(coord.x + 1, coord.y);
        yield return new Vector2Int(coord.x - 1, coord.y);
        yield return new Vector2Int(coord.x, coord.y + 1);
        yield return new Vector2Int(coord.x, coord.y - 1);
    }

    private static bool IsBlocked(Vector2 worldPos, float cellSize, float clearance, LayerMask obstacleMask)
    {
        float inflatedSize = cellSize * 0.8f + Mathf.Max(0f, clearance) * 2f;
        Vector2 size = new Vector2(inflatedSize, inflatedSize);
        return Physics2D.OverlapBox(worldPos, size, 0f, obstacleMask);
    }

    private static Vector2Int FindLowestF(List<Vector2Int> open, Dictionary<Vector2Int, Node> nodes)
    {
        Vector2Int best = open[0];
        float bestF = nodes[best].F;

        for (int i = 1; i < open.Count; i++)
        {
            var coord = open[i];
            float f = nodes[coord].F;
            if (f < bestF)
            {
                best = coord;
                bestF = f;
            }
        }

        return best;
    }

    private static List<Vector2> Reconstruct(Dictionary<Vector2Int, Node> nodes, Vector2Int current, float cellSize)
    {
        var result = new List<Vector2>();
        Vector2Int cursor = current;

        while (nodes.TryGetValue(cursor, out var node))
        {
            result.Add(CoordToWorld(cursor, cellSize));
            if (!node.HasParent)
            {
                break;
            }

            cursor = node.Parent;
        }

        result.Reverse();
        return result;
    }
}
