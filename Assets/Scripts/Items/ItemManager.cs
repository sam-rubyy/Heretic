using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class ItemManager : MonoBehaviour
{
    #region Fields
    [SerializeField] private List<ItemBase> collectedItems = new List<ItemBase>();
    [SerializeField] private PlayerStats playerStats;
    private bool initialized;
    #endregion

    #region Properties
    public IReadOnlyList<ItemBase> CollectedItems => collectedItems;
    public PlayerStats PlayerStats => playerStats;
    #endregion

    #region Unity Methods
    private void OnEnable()
    {
        InitializeExistingItems();
    }
    #endregion

    #region Public Methods
    public void AddItem(ItemBase item, GameObject collector)
    {
        if (item == null)
        {
            return;
        }

        collectedItems.Add(item);
        ApplyPlayerStatModifier(item);
        item.OnCollected(collector);
        GameplayEvents.RaiseItemCollected(item, collector);
    }

    public void RemoveItem(ItemBase item, GameObject collector)
    {
        if (item == null)
        {
            return;
        }

        RemovePlayerStatModifier(item);
        collectedItems.Remove(item);
        item.OnRemoved(collector);
    }

    public ShotParams ApplyShotModifiers(ShotParams shotParams)
    {
        foreach (var item in GetOrderedItems())
        {
            if (item is IShotModifier modifier)
            {
                shotParams = modifier.ModifyShot(shotParams);
            }
        }

        return shotParams;
    }

    public BulletParams ApplyBulletModifiers(BulletParams bulletParams)
    {
        foreach (var item in GetOrderedItems())
        {
            if (item is IBulletModifier modifier)
            {
                bulletParams = modifier.ModifyBullet(bulletParams);
            }
        }

        return bulletParams;
    }
    #endregion

    #region Private Methods
    private void InitializeExistingItems()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;

        for (int i = 0; i < collectedItems.Count; i++)
        {
            var item = collectedItems[i];
            if (item == null)
            {
                continue;
            }

            ApplyPlayerStatModifier(item);
            item.OnCollected(gameObject);
        }
    }

    private IEnumerable<ItemBase> GetOrderedItems()
    {
        return collectedItems
            .OrderBy(item => (item as IItemModifierPriority)?.Priority ?? 0);
    }

    private void ApplyPlayerStatModifier(ItemBase item)
    {
        if (playerStats == null)
        {
            return;
        }

        if (item is IPlayerStatModifier statModifier)
        {
            statModifier.Apply(playerStats);
        }
    }

    private void RemovePlayerStatModifier(ItemBase item)
    {
        if (playerStats == null)
        {
            return;
        }

        if (item is IPlayerStatModifier statModifier)
        {
            statModifier.Remove(playerStats);
        }
    }
    #endregion
}
