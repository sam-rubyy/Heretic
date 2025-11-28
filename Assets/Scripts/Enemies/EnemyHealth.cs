using System;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemyHealth : MonoBehaviour
{
    #region Fields
    [SerializeField] private float maxHealth = 15;
    [SerializeField] private float currentHealth;
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
    public void Initialize(float maxHealthValue)
    {
        maxHealth = Mathf.Max(1, maxHealthValue);
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0 || currentHealth <= 0)
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - amount);

        if (currentHealth <= 0)
        {
            HandleDeath();
        }
    }

    public void Heal(float amount)
    {
        if (amount <= 0 || currentHealth >= maxHealth)
        {
            return;
        }

        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
    }

    public void HandleDeath()
    {
        currentHealth = 0;

        if (owner != null)
        {
            owner.OnDeath();
        }

        Died?.Invoke(owner);
        GameplayEvents.RaiseEnemyDied(owner);
    }
    #endregion
}
