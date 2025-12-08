using UnityEngine;
using TMPro;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class ShopItemInteractable : MonoBehaviour
{
    #region Fields
    [SerializeField] private ShopManager shop;
    [SerializeField, Min(0)] private int stockIndex;
    [SerializeField] private TMP_Text promptText;
    [SerializeField, Tooltip("Prompt format: {0} = item name, {1} = cost.")] private string promptFormat = "Press E to buy {0} ({1})";
    [SerializeField] private GameObject promptRoot;

    private GameObject currentBuyer;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        if (promptText == null && promptRoot != null)
        {
            promptText = promptRoot.GetComponentInChildren<TMP_Text>();
        }

        if (promptRoot == null && promptText != null)
        {
            promptRoot = promptText.gameObject;
        }

        SetPromptVisible(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            currentBuyer = other.gameObject;
            RefreshPrompt();
            SetPromptVisible(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.gameObject == currentBuyer)
        {
            currentBuyer = null;
            SetPromptVisible(false);
        }
    }

    private void Update()
    {
        if (currentBuyer == null || shop == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (shop.TryPurchase(stockIndex, currentBuyer))
            {
                SetPromptVisible(false);
            }
            else
            {
                // Refresh prompt to reflect sold-out or insufficient funds.
                RefreshPrompt();
            }
        }
    }
    #endregion

    #region Private Methods
    private void RefreshPrompt()
    {
        if (promptText == null || shop == null || stockIndex < 0 || stockIndex >= shop.Stock.Count)
        {
            return;
        }

        var entry = shop.Stock[stockIndex];
        if (entry == null)
        {
            return;
        }

        string itemName = entry.Item != null ? entry.Item.DisplayName : "Item";
        string costText = entry.Sold ? "Sold Out" : $"${entry.Cost}";
        promptText.text = string.Format(promptFormat, itemName, costText);
    }

    private void SetPromptVisible(bool visible)
    {
        if (promptRoot != null)
        {
            promptRoot.SetActive(visible);
        }
        else if (promptText != null)
        {
            promptText.gameObject.SetActive(visible);
        }
    }
    #endregion
}
