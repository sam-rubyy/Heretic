using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class RoomManager : MonoBehaviour
{
    #region Fields
    [SerializeField] private List<Room> rooms = new List<Room>();
    [SerializeField] private Room currentRoom;
    #endregion

    #region Unity Methods
    private void Start()
    {
        if (rooms.Count == 0)
        {
            return;
        }

        if (currentRoom == null)
        {
            currentRoom = rooms[0];
        }

        SetActiveRoom(currentRoom);
        GameplayEvents.OnRoomCleared += HandleRoomCleared;
    }

    private void OnDestroy()
    {
        GameplayEvents.OnRoomCleared -= HandleRoomCleared;
    }
    #endregion

    #region Public Methods
    public void LoadNextRoom()
    {
        if (rooms.Count == 0)
        {
            return;
        }

        int currentIndex = rooms.IndexOf(currentRoom);
        if (currentIndex < 0)
        {
            currentIndex = 0;
        }

        int nextIndex = currentIndex + 1;
        if (nextIndex >= rooms.Count)
        {
            RestartRun();
            return;
        }

        SetActiveRoom(rooms[nextIndex]);
    }

    public void RestartRun()
    {
        if (rooms.Count == 0)
        {
            return;
        }

        SetActiveRoom(rooms[0]);
    }
    #endregion

    #region Private Methods
    private void HandleRoomCleared(Room room)
    {
        if (room == currentRoom)
        {
            LoadNextRoom();
        }
    }

    private void SetActiveRoom(Room room)
    {
        currentRoom = room;

        for (int i = 0; i < rooms.Count; i++)
        {
            var targetRoom = rooms[i];
            if (targetRoom == null)
            {
                continue;
            }

            bool shouldBeActive = targetRoom == currentRoom;
            if (targetRoom.gameObject.activeSelf != shouldBeActive)
            {
                targetRoom.gameObject.SetActive(shouldBeActive);
            }
        }
    }
    #endregion
}
