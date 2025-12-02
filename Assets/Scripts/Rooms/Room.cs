using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Room : MonoBehaviour
{
    #region Fields
    [SerializeField] private List<EnemyBase> enemies = new List<EnemyBase>();
    [SerializeField] private List<EnemySpawner> enemySpawners = new List<EnemySpawner>();
    [SerializeField] private bool autoSpawnOnActivate = true;
    [SerializeField] private bool lockDoorsUntilCleared = true;
    private readonly List<RoomDoor> doors = new List<RoomDoor>();
    private bool encounterSpawned;
    private RoomTemplate template;
    private FloorConfig floorConfig;
    private int depth;
    private bool componentsCached;
    #endregion

    #region Properties
    public bool IsCleared => enemies.Count == 0;
    public RoomTemplate Template => template;
    public int Depth => depth;
    #endregion

    #region Public Methods
    public void Configure(RoomTemplate templateValue, FloorConfig config, int depthValue)
    {
        template = templateValue;
        floorConfig = config;
        depth = Mathf.Max(0, depthValue);

        CacheComponents();

        for (int i = 0; i < enemySpawners.Count; i++)
        {
            var spawner = enemySpawners[i];
            if (spawner != null)
            {
                spawner.SetOwningRoom(this);
            }
        }
    }

    public void OnActivated()
    {
        if (lockDoorsUntilCleared)
        {
            SetDoorsLocked(!IsCleared);
        }

        if (autoSpawnOnActivate && !encounterSpawned)
        {
            SpawnEncounter();
        }
    }

    public void SpawnEncounter()
    {
        if (encounterSpawned)
        {
            return;
        }

        CacheComponents();

        if (floorConfig == null)
        {
            // Fallback: just fire each spawner once using its default prefab.
            for (int i = 0; i < enemySpawners.Count; i++)
            {
                var spawner = enemySpawners[i];
                if (spawner != null)
                {
                    spawner.SetOwningRoom(this);
                    spawner.SpawnEnemies(1);
                }
            }

            encounterSpawned = true;
            return;
        }

        encounterSpawned = true;

        int enemyCount = floorConfig.GetEnemyCount(template, depth);
        float targetDifficulty = floorConfig.GetDifficultyForDepth(depth, template);

        for (int i = 0; i < enemyCount; i++)
        {
            EnemySpawner spawner = enemySpawners.Count > 0 ? enemySpawners[i % enemySpawners.Count] : null;
            if (spawner == null)
            {
                continue;
            }

            spawner.SetOwningRoom(this);

            EnemyBase enemyPrefab = floorConfig.GetEnemyForDifficulty(targetDifficulty);
            if (enemyPrefab == null)
            {
                continue;
            }

            spawner.SpawnEnemy(enemyPrefab, spawner.transform.position, this);
        }
    }

    public void RegisterEnemy(EnemyBase enemy)
    {
        if (enemy == null || enemies.Contains(enemy))
        {
            return;
        }

        enemies.Add(enemy);
    }

    public void UnregisterEnemy(EnemyBase enemy)
    {
        if (enemy == null)
        {
            return;
        }

        enemies.Remove(enemy);

        if (IsCleared)
        {
            UnlockDoors();
            GameplayEvents.RaiseRoomCleared(this);
        }
    }

    public void SetDoorsLocked(bool locked)
    {
        CacheComponents();

        for (int i = 0; i < doors.Count; i++)
        {
            var door = doors[i];
            if (door != null)
            {
                door.SetLocked(locked);
            }
        }
    }

    public void UnlockDoors()
    {
        SetDoorsLocked(false);
    }
    #endregion

    #region Private Methods
    private void Awake()
    {
        CacheComponents();
    }

    private void CacheComponents()
    {
        if (!componentsCached)
        {
            doors.Clear();
            doors.AddRange(GetComponentsInChildren<RoomDoor>(true));
            componentsCached = true;
        }

        enemySpawners.RemoveAll(spawner => spawner == null);

        var foundSpawners = GetComponentsInChildren<EnemySpawner>(true);
        for (int i = 0; i < foundSpawners.Length; i++)
        {
            var spawner = foundSpawners[i];
            if (spawner != null && !enemySpawners.Contains(spawner))
            {
                enemySpawners.Add(spawner);
            }
        }
    }
    #endregion
}
