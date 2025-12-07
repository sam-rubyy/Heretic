using UnityEngine;

[DisallowMultipleComponent]
public class TreasureRoomDrop : MonoBehaviour
{
    #region Fields
    [SerializeField] private EnemyLootDropper dropper;
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private bool dropOnlyOnce = true;
    private bool hasDropped;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        if (dropper == null)
        {
            dropper = GetComponent<EnemyLootDropper>();
        }
    }

    private void Start()
    {
        if (spawnOnStart)
        {
            SpawnLoot();
        }
    }
    #endregion

    #region Public Methods
    public void SpawnLoot()
    {
        if (dropOnlyOnce && hasDropped)
        {
            return;
        }

        if (dropper == null)
        {
            return;
        }

        dropper.DropLoot();
        hasDropped = true;
    }
    #endregion
}
