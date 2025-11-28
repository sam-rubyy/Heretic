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
        Destroy(gameObject);
    }
    #endregion
}
