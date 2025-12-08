using UnityEngine;

[DisallowMultipleComponent]
public class PlayerWallet : MonoBehaviour
{
    #region Fields
    [SerializeField, Min(0)] private int startingMoney = 0;
    [SerializeField, Min(0)] private int currentMoney;
    #endregion

    #region Properties
    public int CurrentMoney => currentMoney;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        currentMoney = Mathf.Max(0, startingMoney);
        GameplayEvents.RaiseMoneyChanged(currentMoney);
    }
    #endregion

    #region Public Methods
    public bool TrySpend(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (currentMoney < amount)
        {
            return false;
        }

        currentMoney -= amount;
        GameplayEvents.RaiseMoneyChanged(currentMoney);
        return true;
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentMoney += amount;
        GameplayEvents.RaiseMoneyChanged(currentMoney);
    }
    #endregion
}
