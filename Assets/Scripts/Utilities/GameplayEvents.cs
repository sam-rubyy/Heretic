using System;

public static class GameplayEvents
{
    #region Events
    public static event Action<EnemyBase> OnEnemyDied;
    public static event Action<Room> OnRoomCleared;
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
    #endregion
}
