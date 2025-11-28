using UnityEngine;

[DisallowMultipleComponent]
public class EnemyBase : MonoBehaviour
{
    #region Fields
    [SerializeField] private EnemyHealth health;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        if (health == null)
        {
            health = GetComponent<EnemyHealth>();
        }
    }
    #endregion

    #region Public Methods
    public virtual void Initialize()
    {
    }

    public virtual void OnSpawned()
    {
    }

    public virtual void OnDeath()
    {
    }
    #endregion
}
