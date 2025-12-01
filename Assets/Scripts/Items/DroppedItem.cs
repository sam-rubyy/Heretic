using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class DroppedItem : MonoBehaviour
{
    #region Fields
    [SerializeField] private ItemBase item;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite fallbackSprite;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        UpdateVisual();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryCollect(other?.gameObject);
    }
    #endregion

    #region Public Methods
    public void Initialize(ItemBase newItem)
    {
        item = newItem;
        UpdateVisual();
    }
    #endregion

    #region Private Methods
    private void UpdateVisual()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        Sprite spriteToUse = item != null && item.Icon != null ? item.Icon : fallbackSprite;
        if (spriteToUse != null)
        {
            spriteRenderer.sprite = spriteToUse;
        }
    }

    private void TryCollect(GameObject collector)
    {
        if (item == null || collector == null)
        {
            return;
        }

        var itemManager = collector.GetComponentInParent<ItemManager>();
        if (itemManager == null)
        {
            return;
        }

        itemManager.AddItem(item, collector);
        Destroy(gameObject);
    }
    #endregion
}
