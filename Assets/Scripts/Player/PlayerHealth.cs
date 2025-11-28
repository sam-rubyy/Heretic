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
    }
    #endregion

    #region Public Methods
    public void TakeDamage(int amount)
    {
    }

    public void Heal(int amount)
    {
    }

    public void ResetHealth()
    {
    }
    #endregion
}
