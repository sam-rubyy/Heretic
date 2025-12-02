using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Heretic/Rooms/Floor Config")]
public class FloorConfig : ScriptableObject
{
    #region Types
    [Serializable]
    public struct EnemyPoolEntry
    {
        public EnemyBase enemyPrefab;
        [Min(1)] public int weight;
        public int minDifficulty;
        public int maxDifficulty;

        public bool SupportsDifficulty(float difficulty)
        {
            return difficulty >= minDifficulty && difficulty <= maxDifficulty;
        }
    }
    #endregion

    #region Fields
    [Header("Templates")]
    [SerializeField] private RoomTemplate startRoom;
    [SerializeField] private RoomTemplate bossRoom;
    [SerializeField] private List<RoomTemplate> normalRooms = new List<RoomTemplate>();
    [SerializeField] private List<RoomTemplate> specialRooms = new List<RoomTemplate>();

    [Header("Layout")]
    [SerializeField, Min(2)] private Vector2Int mainPathLength = new Vector2Int(4, 7);
    [SerializeField, Min(0)] private int maxBranches = 2;
    [SerializeField, Min(1)] private int branchLength = 1;

    [Header("Enemies & Difficulty")]
    [SerializeField] private Vector2Int baseEnemyCountRange = new Vector2Int(2, 4);
    [SerializeField, Min(0)] private int additionalEnemiesPerDepth = 0;
    [SerializeField, Min(1)] private int bossEnemyCount = 1;
    [SerializeField] private float baseDifficulty = 1f;
    [SerializeField] private float difficultyPerDepth = 0.5f;
    [SerializeField] private List<EnemyPoolEntry> enemyPool = new List<EnemyPoolEntry>();
    #endregion

    #region Properties
    public Vector2Int MainPathLength => mainPathLength;
    public int MaxBranches => Mathf.Max(0, maxBranches);
    public int BranchLength => Mathf.Max(1, branchLength);
    public RoomTemplate StartRoom => startRoom;
    public RoomTemplate BossRoom => bossRoom;
    #endregion

    #region Public Methods
    public RoomTemplate GetRandomNormalTemplate(RoomTemplate.DoorLayout requiredDoors, int depth)
    {
        return GetWeightedTemplate(normalRooms, requiredDoors, depth);
    }

    public RoomTemplate GetRandomSpecialTemplate(RoomTemplate.DoorLayout requiredDoors, int depth)
    {
        return GetWeightedTemplate(specialRooms, requiredDoors, depth);
    }

    public int GetEnemyCount(RoomTemplate template, int depth)
    {
        if (template != null && template.RoomType == RoomType.Boss)
        {
            return bossEnemyCount;
        }

        int min = baseEnemyCountRange.x + depth * additionalEnemiesPerDepth;
        int max = baseEnemyCountRange.y + depth * additionalEnemiesPerDepth;
        if (max < min)
        {
            max = min;
        }

        return UnityEngine.Random.Range(Mathf.Max(1, min), Mathf.Max(min + 1, max + 1));
    }

    public float GetDifficultyForDepth(int depth, RoomTemplate template = null)
    {
        float difficulty = baseDifficulty + depth * difficultyPerDepth;
        if (template != null)
        {
            difficulty += template.DifficultyRating * 0.5f;
        }

        return difficulty;
    }

    public EnemyBase GetEnemyForDifficulty(float difficulty)
    {
        var candidates = new List<EnemyPoolEntry>();
        for (int i = 0; i < enemyPool.Count; i++)
        {
            var entry = enemyPool[i];
            if (entry.enemyPrefab == null || entry.weight <= 0)
            {
                continue;
            }

            if (entry.SupportsDifficulty(difficulty))
            {
                candidates.Add(entry);
            }
        }

        if (candidates.Count == 0)
        {
            return null;
        }

        int totalWeight = 0;
        for (int i = 0; i < candidates.Count; i++)
        {
            totalWeight += Mathf.Max(1, candidates[i].weight);
        }

        int roll = UnityEngine.Random.Range(0, totalWeight);
        for (int i = 0; i < candidates.Count; i++)
        {
            int weight = Mathf.Max(1, candidates[i].weight);
            if (roll < weight)
            {
                return candidates[i].enemyPrefab;
            }

            roll -= weight;
        }

        return candidates[0].enemyPrefab;
    }
    #endregion

    #region Private Methods
    private RoomTemplate GetWeightedTemplate(List<RoomTemplate> pool, RoomTemplate.DoorLayout requiredDoors, int depth)
    {
        var filtered = new List<RoomTemplate>();
        for (int i = 0; i < pool.Count; i++)
        {
            RoomTemplate template = pool[i];
            if (template == null)
            {
                continue;
            }

            if (!template.SupportsDoors(requiredDoors))
            {
                continue;
            }

            if (depth < template.MinDepth || depth > template.MaxDepth)
            {
                continue;
            }

            filtered.Add(template);
        }

        if (filtered.Count == 0)
        {
            return null;
        }

        int totalWeight = 0;
        for (int i = 0; i < filtered.Count; i++)
        {
            totalWeight += Mathf.Max(1, filtered[i].Weight);
        }

        int roll = UnityEngine.Random.Range(0, totalWeight);
        for (int i = 0; i < filtered.Count; i++)
        {
            int weight = Mathf.Max(1, filtered[i].Weight);
            if (roll < weight)
            {
                return filtered[i];
            }

            roll -= weight;
        }

        return filtered[0];
    }
    #endregion
}
