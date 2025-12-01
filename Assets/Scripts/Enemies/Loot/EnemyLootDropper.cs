using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemyLootDropper : MonoBehaviour
{
    #region Fields
    [SerializeField] private EnemyLootPool lootPool;
    [SerializeField] private float dropChance = 1f;
    [SerializeField] private int rollCount = 1;
    [SerializeField] private bool preventDuplicateDrops = true;
    [SerializeField] private ItemBase[] guaranteedDrops;
    [SerializeField] private DroppedItem dropPrefab;
    [SerializeField] private Vector2 dropScatter = new Vector2(0.75f, 0.75f);
    private EnemyBase owner;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        owner = GetComponent<EnemyBase>();
    }
    #endregion

    #region Public Methods
    public void SetLootPool(EnemyLootPool pool)
    {
        lootPool = pool;
    }

    public List<ItemBase> DropLoot()
    {
        return DropLoot(lootPool, guaranteedDrops, rollCount, dropChance, preventDuplicateDrops);
    }

    public List<ItemBase> DropLoot(
        EnemyLootPool poolOverride,
        IEnumerable<ItemBase> guaranteedOverride,
        int rollOverride = -1,
        float? dropChanceOverride = null,
        bool preventDuplicates = true)
    {
        var drops = new List<ItemBase>();

        var pool = poolOverride != null ? poolOverride : lootPool;
        int rollsToUse = rollOverride > 0 ? rollOverride : rollCount;
        float chanceToUse = Mathf.Clamp01(dropChanceOverride ?? dropChance);

        AddGuaranteedDrops(drops, guaranteedOverride, preventDuplicates);

        if (pool != null && rollsToUse > 0 && Random.value <= chanceToUse)
        {
            for (int i = 0; i < rollsToUse; i++)
            {
                ItemBase drop = pool.GetRandomItem(preventDuplicates ? drops : null);
                if (drop == null)
                {
                    continue;
                }

                if (preventDuplicates && drops.Contains(drop))
                {
                    continue;
                }

                drops.Add(drop);
            }
        }

        NotifyDrops(drops);
        SpawnDrops(drops);
        return drops;
    }
    #endregion

    #region Private Methods
    private void AddGuaranteedDrops(List<ItemBase> drops, IEnumerable<ItemBase> guaranteed, bool preventDuplicates)
    {
        if (guaranteed == null)
        {
            return;
        }

        foreach (var item in guaranteed)
        {
            if (item == null)
            {
                continue;
            }

            if (preventDuplicates && drops.Contains(item))
            {
                continue;
            }

            drops.Add(item);
        }
    }

    private void NotifyDrops(List<ItemBase> drops)
    {
        if (drops == null || drops.Count == 0)
        {
            return;
        }

        for (int i = 0; i < drops.Count; i++)
        {
            GameplayEvents.RaiseEnemyDroppedItem(owner, drops[i]);
        }
    }

    private void SpawnDrops(List<ItemBase> drops)
    {
        if (drops == null || drops.Count == 0)
        {
            return;
        }

        if (dropPrefab == null)
        {
            return;
        }

        Vector3 basePos = transform.position;

        for (int i = 0; i < drops.Count; i++)
        {
            Vector2 offset = new Vector2(
                Random.Range(-dropScatter.x, dropScatter.x),
                Random.Range(-dropScatter.y, dropScatter.y));

            var dropInstance = Instantiate(dropPrefab, basePos + (Vector3)offset, Quaternion.identity);
            dropInstance.Initialize(drops[i]);
        }
    }
    #endregion
}
