using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LootPool", menuName = "Enemies/Loot Pool")]
public class EnemyLootPool : ScriptableObject
{
    #region Fields
    [SerializeField] private List<LootEntry> entries = new List<LootEntry>();
    #endregion

    #region Public Methods
    public ItemBase GetRandomItem(ICollection<ItemBase> exclude = null)
    {
        float totalWeight = 0f;
        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            if (entry == null || entry.Item == null)
            {
                continue;
            }

            if (exclude != null && exclude.Contains(entry.Item))
            {
                continue;
            }

            totalWeight += entry.Weight;
        }

        if (totalWeight <= 0.0001f)
        {
            return null;
        }

        float roll = UnityEngine.Random.value * totalWeight;
        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            if (entry == null || entry.Item == null)
            {
                continue;
            }

            if (exclude != null && exclude.Contains(entry.Item))
            {
                continue;
            }

            roll -= entry.Weight;
            if (roll <= 0f)
            {
                return entry.Item;
            }
        }

        return null;
    }
    #endregion
}

[Serializable]
public class LootEntry
{
    #region Fields
    [SerializeField] private ItemBase item;
    [SerializeField] private float weight = 1f;
    #endregion

    #region Properties
    public ItemBase Item => item;
    public float Weight => Mathf.Max(0.01f, weight);
    #endregion
}
