using System;
using UnityEngine;

public static class GameplayEvents
{
    #region Events
    public static event Action<EnemyBase> OnEnemyDied;
    public static event Action<Room> OnRoomCleared;
    public static event Action<EnemyBase, ItemBase> OnEnemyDroppedItem;
    public static event Action<ItemBase, GameObject> OnItemCollected;
    public static event Action<PlayerHealth> OnPlayerDied;
    #endregion

    #region Public Methods
    public static void RaiseEnemyDied(EnemyBase enemy)
    {
        OnEnemyDied?.Invoke(enemy);
    }

    public static void RaiseRoomCleared(Room room)
    {
        OnRoomCleared?.Invoke(room);
    }

    public static void RaiseEnemyDroppedItem(EnemyBase enemy, ItemBase item)
    {
        OnEnemyDroppedItem?.Invoke(enemy, item);
    }

    public static void RaiseItemCollected(ItemBase item, GameObject collector)
    {
        OnItemCollected?.Invoke(item, collector);
    }

    public static void RaisePlayerDied(PlayerHealth player)
    {
        OnPlayerDied?.Invoke(player);
    }
    #endregion
}
