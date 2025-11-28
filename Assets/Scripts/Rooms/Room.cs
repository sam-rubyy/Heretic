using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Room : MonoBehaviour
{
    #region Fields
    [SerializeField] private List<EnemyBase> enemies = new List<EnemyBase>();
    #endregion

    #region Properties
    public bool IsCleared => enemies.Count == 0;
    #endregion

    #region Public Methods
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
            GameplayEvents.RaiseRoomCleared(this);
        }
    }
    #endregion
}
