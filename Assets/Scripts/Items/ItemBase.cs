using UnityEngine;

public abstract class ItemBase : ScriptableObject
{
    #region Fields
    [SerializeField] private string itemId;
    [SerializeField] private string displayName;
    [TextArea] [SerializeField] private string description;
    [SerializeField] private Sprite icon;
    #endregion

    #region Properties
    public string ItemId => itemId;
    public string DisplayName => displayName;
    public string Description => description;
    public Sprite Icon => icon;
    #endregion

    #region Public Methods
    public virtual void OnCollected(GameObject collector)
    {
    }

    public virtual void OnRemoved(GameObject collector)
    {
    }
    #endregion
}
