using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ShopManager : MonoBehaviour
{
    #region Fields
    [SerializeField] private List<ShopEntry> stock = new List<ShopEntry>();
    [Header("Random Stock")]
    [SerializeField, Tooltip("Pool of items to choose from when restocking randomly.")] private List<ItemBase> itemPool = new List<ItemBase>();
    [SerializeField, Tooltip("Inclusive min/max cost when generating random entries.")] private Vector2Int randomCostRange = new Vector2Int(5, 15);
    [SerializeField, Tooltip("Prevent duplicate items when randomly restocking.")] private bool preventDuplicateItems = true;
    #endregion

    #region Properties
    public IReadOnlyList<ShopEntry> Stock => stock;
    #endregion

    #region Public Methods
    public bool TryPurchase(int index, GameObject buyer)
    {
        if (index < 0 || index >= stock.Count || buyer == null)
        {
            return false;
        }

        var entry = stock[index];
        if (entry == null || entry.Sold || entry.Item == null)
        {
            return false;
        }

        var wallet = buyer.GetComponentInParent<PlayerWallet>();
        if (wallet == null || !wallet.TrySpend(entry.Cost))
        {
            return false;
        }

        var itemManager = buyer.GetComponentInParent<ItemManager>();
        if (itemManager != null)
        {
            itemManager.AddItem(entry.Item, buyer);
        }

        entry.MarkSold();
        return true;
    }

    public void Restock(IEnumerable<ShopEntry> newStock)
    {
        stock.Clear();
        if (newStock == null)
        {
            return;
        }

        stock.AddRange(newStock);
    }

    public void RestockRandom(int slotCount = -1)
    {
        stock.Clear();

        if (itemPool == null || itemPool.Count == 0)
        {
            return;
        }

        int count = slotCount > 0 ? slotCount : Mathf.Max(1, stock.Count > 0 ? stock.Count : 3);

        var used = preventDuplicateItems ? new HashSet<ItemBase>() : null;
        for (int i = 0; i < count; i++)
        {
            ItemBase item = GetRandomItem(used);
            if (item == null)
            {
                continue;
            }

            used?.Add(item);
            int cost = UnityEngine.Random.Range(Mathf.Max(0, randomCostRange.x), Mathf.Max(randomCostRange.x, randomCostRange.y) + 1);
            stock.Add(new ShopEntry(item, cost));
        }
    }
    #endregion

    #region Private Methods
    private ItemBase GetRandomItem(HashSet<ItemBase> used)
    {
        if (itemPool == null || itemPool.Count == 0)
        {
            return null;
        }

        // Simple retry loop to avoid duplicates if requested.
        for (int attempt = 0; attempt < itemPool.Count * 2; attempt++)
        {
            var candidate = itemPool[UnityEngine.Random.Range(0, itemPool.Count)];
            if (candidate == null)
            {
                continue;
            }

            if (used != null && used.Contains(candidate))
            {
                continue;
            }

            return candidate;
        }

        return null;
    }
    #endregion
}

[Serializable]
public class ShopEntry
{
    #region Fields
    [SerializeField] private ItemBase item;
    [SerializeField, Min(0)] private int cost = 5;
    [SerializeField] private bool sold;
    #endregion

    #region Properties
    public ItemBase Item => item;
    public int Cost => cost;
    public bool Sold => sold;
    #endregion

    #region Public Methods
    public void MarkSold()
    {
        sold = true;
    }

    public void ResetSold()
    {
        sold = false;
    }

    public ShopEntry()
    {
    }

    public ShopEntry(ItemBase item, int cost)
    {
        this.item = item;
        this.cost = Mathf.Max(0, cost);
        sold = false;
    }
    #endregion
}
