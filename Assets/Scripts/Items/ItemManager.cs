using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ItemManager : MonoBehaviour
{
    #region Fields
    [SerializeField] private List<ItemBase> collectedItems = new List<ItemBase>();
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
        return shotParams;
    }

    public BulletParams ApplyBulletModifiers(BulletParams bulletParams)
    {
        return bulletParams;
    }
    #endregion
}
