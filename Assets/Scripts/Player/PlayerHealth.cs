using UnityEngine;

[DisallowMultipleComponent]
public class PlayerHealth : MonoBehaviour
{
    #region Fields
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private int currentHealth;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        currentHealth = Mathf.Max(0, maxHealth);
    }
    #endregion

    #region Public Methods
    public void TakeDamage(int amount)
    {
        if (amount <= 0 || currentHealth <= 0)
        {
            return;
        }

        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);

        if (currentHealth <= 0)
        {
            // Future death handling can be added here (animations, events, etc).
        }
    }

    public void Heal(int amount)
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
    #endregion
}
