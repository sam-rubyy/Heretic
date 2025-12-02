using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemySpawner : MonoBehaviour
{
    #region Fields
    [SerializeField] private EnemyBase enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Room owningRoom;
    private readonly List<EnemyBase> activeEnemies = new List<EnemyBase>();
    #endregion

    #region Public Methods
    public void SpawnEnemies(int count)
    {
        if (enemyPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            Transform spawnPoint = spawnPoints[i % spawnPoints.Length];
            SpawnEnemy(enemyPrefab, spawnPoint.position, owningRoom);
        }
    }

    public void SpawnEnemy(EnemyBase prefab, Vector3 position, Room roomOverride = null)
    {
        var prefabToUse = prefab == null ? enemyPrefab : prefab;
        if (prefabToUse == null)
        {
            return;
        }

        EnemyBase enemyInstance = Instantiate(prefabToUse, position, Quaternion.identity);
        if (enemyInstance == null)
        {
            return;
        }

        enemyInstance.transform.SetParent(transform);
        activeEnemies.Add(enemyInstance);

        Room room = roomOverride != null ? roomOverride : owningRoom;
        if (room != null)
        {
            room.RegisterEnemy(enemyInstance);
        }

        var health = enemyInstance.GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.ResetHealth();
            health.Died += HandleEnemyDied;
        }

        enemyInstance.Initialize();
        enemyInstance.OnSpawned();
    }

    public void ClearActiveEnemies()
    {
        for (int i = 0; i < activeEnemies.Count; i++)
        {
            var enemy = activeEnemies[i];
            if (enemy == null)
            {
                continue;
            }

            if (owningRoom != null)
            {
                owningRoom.UnregisterEnemy(enemy);
            }

            var health = enemy.GetComponent<EnemyHealth>();
            if (health != null)
            {
                health.Died -= HandleEnemyDied;
            }

            Destroy(enemy.gameObject);
        }

        activeEnemies.Clear();
    }
    #endregion

    #region Private Methods
    private void HandleEnemyDied(EnemyBase enemy)
    {
        if (enemy == null)
        {
            return;
        }

        activeEnemies.Remove(enemy);

        var health = enemy.GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.Died -= HandleEnemyDied;
        }

        if (owningRoom != null)
        {
            owningRoom.UnregisterEnemy(enemy);
        }
    }
    #endregion

    #region Configuration
    public void SetOwningRoom(Room room)
    {
        owningRoom = room;
    }
    #endregion
}
