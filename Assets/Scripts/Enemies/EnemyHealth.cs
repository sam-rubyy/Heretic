using System;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemyHealth : MonoBehaviour
{
    #region Fields
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int currentHealth;
    private EnemyBase owner;
    #endregion

    #region Events
    public event Action<EnemyBase> Died;
    #endregion
    

    #region Unity Methods
    private void Awake()
    {
        owner = GetComponent<EnemyBase>();
    }
    #endregion

    #region Public Methods
    public void Initialize(int maxHealthValue)
    {
        maxHealth = maxHealthValue;
    }

    public void TakeDamage(int amount)
    {
    }

    public void Heal(int amount)
    {
    }

    public void ResetHealth()
    {
    }

    public void HandleDeath()
    {
        Died?.Invoke(owner);
        GameplayEvents.RaiseEnemyDied(owner);
    }
    #endregion
}
