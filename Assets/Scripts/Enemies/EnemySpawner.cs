using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemySpawner : MonoBehaviour
{
    #region Fields
    [SerializeField] private EnemyBase enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;
    private readonly List<EnemyBase> activeEnemies = new List<EnemyBase>();
    #endregion

    #region Public Methods
    public void SpawnEnemies(int count)
    {
    }

    public void SpawnEnemy(EnemyBase prefab, Vector3 position)
    {
    }

    public void ClearActiveEnemies()
    {
        activeEnemies.Clear();
    }
    #endregion
}
