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
    }
    #endregion

    #region Public Methods
    public void LoadNextRoom()
    {
    }

    public void RestartRun()
    {
    }
    #endregion
}
