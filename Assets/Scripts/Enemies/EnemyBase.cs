using UnityEngine;

[DisallowMultipleComponent]
public class EnemyBase : MonoBehaviour
{
    #region Fields
    [SerializeField] private EnemyHealth health;
    [SerializeField] private EnemyLootDropper lootDropper;
    #endregion

    #region Properties
    protected EnemyLootDropper LootDropper => lootDropper;
    #endregion

    #region Unity Methods
    protected virtual void Awake()
    {
        if (health == null)
        {
            health = GetComponent<EnemyHealth>();
        }

        if (lootDropper == null)
        {
            lootDropper = GetComponent<EnemyLootDropper>();
        }
    }
    #endregion

    #region Public Methods
    public virtual void Initialize()
    {
        if (health == null)
        {
            health = GetComponent<EnemyHealth>();
        }

        if (health != null)
        {
            health.ResetHealth();
        }
    }

    public virtual void OnSpawned()
    {
        gameObject.SetActive(true);
    }

    public virtual void OnDeath()
    {
        DropLoot();
        Destroy(gameObject);
    }
    #endregion

    #region Protected Methods
    protected virtual void DropLoot()
    {
        if (lootDropper == null)
        {
            return;
        }

        lootDropper.DropLoot();
    }
    #endregion
}
