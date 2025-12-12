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
    [Header("Money Drops")]
    [SerializeField, Tooltip("Chance that this enemy drops money.")] private float moneyDropChance = 0.4f;
    [SerializeField, Tooltip("Min/Max money dropped when it happens.")] private Vector2Int moneyAmountRange = new Vector2Int(1, 3);
    [SerializeField, Tooltip("Weighted coin prefabs to roll when dropping money.")] private List<MoneyDropOption> moneyDropOptions = new List<MoneyDropOption>();
    [SerializeField, Tooltip("If true, overrides the prefab's amount with the random range.")] private bool overridePrefabAmount = true;
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
        TryDropMoney();
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

    private void TryDropMoney()
    {
        if (moneyDropOptions == null || moneyDropOptions.Count == 0)
        {
            return;
        }

        if (Random.value > Mathf.Clamp01(moneyDropChance))
        {
            return;
        }

        int min = Mathf.Max(1, moneyAmountRange.x);
        int max = Mathf.Max(min, moneyAmountRange.y);
        int amount = Random.Range(min, max + 1);

        Vector2 offset = new Vector2(
            Random.Range(-dropScatter.x, dropScatter.x),
            Random.Range(-dropScatter.y, dropScatter.y));

        MoneyDropOption option = ChooseMoneyOption();
        if (option == null || option.Prefab == null)
        {
            return;
        }

        var money = Instantiate(option.Prefab, transform.position + (Vector3)offset, Quaternion.identity);
        if (overridePrefabAmount && money != null)
        {
            money.SetAmount(amount);
        }
    }

    private MoneyDropOption ChooseMoneyOption()
    {
        float totalWeight = 0f;
        for (int i = 0; i < moneyDropOptions.Count; i++)
        {
            var option = moneyDropOptions[i];
            if (option == null || option.Prefab == null)
            {
                continue;
            }

            totalWeight += Mathf.Max(0f, option.Weight);
        }

        if (totalWeight <= 0f)
        {
            return null;
        }

        float roll = Random.value * totalWeight;
        for (int i = 0; i < moneyDropOptions.Count; i++)
        {
            var option = moneyDropOptions[i];
            if (option == null || option.Prefab == null)
            {
                continue;
            }

            float weight = Mathf.Max(0f, option.Weight);
            if (weight <= 0f)
            {
                continue;
            }

            if (roll <= weight)
            {
                return option;
            }

            roll -= weight;
        }

        return null;
    }
    #endregion
}

[System.Serializable]
public class MoneyDropOption
{
    [SerializeField] private MoneyPickup prefab;
    [SerializeField, Min(0f)] private float weight = 1f;

    public MoneyPickup Prefab => prefab;
    public float Weight => weight;
}
