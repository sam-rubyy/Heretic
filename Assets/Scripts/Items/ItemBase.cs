using UnityEngine;

public abstract class ItemBase : ScriptableObject
{
    #region Fields
    [SerializeField] private string itemId;
    [SerializeField] private string displayName;
    [TextArea] [SerializeField] private string description;
    #endregion

    #region Properties
    public string ItemId => itemId;
    public string DisplayName => displayName;
    public string Description => description;
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
