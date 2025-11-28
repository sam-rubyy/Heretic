using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ItemManager : MonoBehaviour
{
    #region Fields
    [SerializeField] private List<ItemBase> collectedItems = new List<ItemBase>();
    #endregion

    #region Properties
    public IReadOnlyList<ItemBase> CollectedItems => collectedItems;
    #endregion

    #region Public Methods
    public void AddItem(ItemBase item, GameObject collector)
    {
        if (item == null)
        {
            return;
        }

        collectedItems.Add(item);
        item.OnCollected(collector);
    }

    public void RemoveItem(ItemBase item, GameObject collector)
    {
        if (item == null)
        {
            return;
        }

        collectedItems.Remove(item);
        item.OnRemoved(collector);
    }

    public ShotParams ApplyShotModifiers(ShotParams shotParams)
    {
        for (int i = 0; i < collectedItems.Count; i++)
        {
            var item = collectedItems[i];
            if (item is IShotModifier modifier)
            {
                shotParams = modifier.ModifyShot(shotParams);
            }
        }

        return shotParams;
    }

    public BulletParams ApplyBulletModifiers(BulletParams bulletParams)
    {
        for (int i = 0; i < collectedItems.Count; i++)
        {
            var item = collectedItems[i];
            if (item is IBulletModifier modifier)
            {
                bulletParams = modifier.ModifyBullet(bulletParams);
            }
        }

        return bulletParams;
    }
    #endregion
}
