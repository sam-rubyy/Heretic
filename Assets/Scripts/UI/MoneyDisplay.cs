using UnityEngine;
using TMPro;

[DisallowMultipleComponent]
public class MoneyDisplay : MonoBehaviour
{
    #region Fields
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private PlayerWallet wallet;
    [SerializeField] private string prefix = "$";
    #endregion

    #region Unity Methods
    private void Awake()
    {
        if (moneyText == null)
        {
            moneyText = GetComponentInChildren<TMP_Text>();
        }
    }

    private void OnEnable()
    {
        GameplayEvents.OnMoneyChanged += HandleMoneyChanged;
        Refresh();
    }

    private void OnDisable()
    {
        GameplayEvents.OnMoneyChanged -= HandleMoneyChanged;
    }
    #endregion

    #region Private Methods
    private void HandleMoneyChanged(int amount)
    {
        SetMoney(amount);
    }

    private void Refresh()
    {
        if (wallet == null)
        {
            wallet = FindObjectOfType<PlayerWallet>();
        }

        if (wallet != null)
        {
            SetMoney(wallet.CurrentMoney);
        }
    }

    private void SetMoney(int amount)
    {
        if (moneyText == null)
        {
            return;
        }

        moneyText.text = string.IsNullOrEmpty(prefix)
            ? amount.ToString()
            : $"{prefix}{amount}";
    }
    #endregion
}
