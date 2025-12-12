using System.Collections.Generic;
using UnityEngine;
using TMPro;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class ShopItemInteractable : MonoBehaviour
{
    #region Fields
    [Header("Item Setup")]
    [SerializeField] private ItemBase item;
    [SerializeField, Tooltip("Pick a random item/cost at runtime.")] private bool randomizeOnAwake = true;
    [SerializeField, Tooltip("Pool to pull from when randomizing.")] private List<ItemBase> lootPool = new List<ItemBase>();
    [SerializeField, Min(0)] private int cost = 5;
    [SerializeField, Tooltip("Inclusive min/max cost when randomizing.")] private Vector2Int randomCostRange = new Vector2Int(5, 15);
    [SerializeField] private bool sold;
    [SerializeField] private TMP_Text promptText;
    [SerializeField, Tooltip("Prompt format: {0} = item name, {1} = cost or status.")] private string promptFormat = "Press E to buy {0} ({1})";
    [SerializeField] private GameObject promptRoot;

    private GameObject currentBuyer;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        if (randomizeOnAwake || item == null)
        {
            RandomizeItem();
        }

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
        if (currentBuyer == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (TryPurchase(currentBuyer))
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
        if (promptText == null)
        {
            return;
        }

        string itemName = item != null ? item.DisplayName : "Item";
        string costText = sold ? "Sold Out" : $"${cost}";
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

    private void RandomizeItem()
    {
        if (lootPool == null || lootPool.Count == 0)
        {
            return;
        }

        // Try a few picks to avoid null entries.
        for (int attempt = 0; attempt < lootPool.Count * 2; attempt++)
        {
            var candidate = lootPool[Random.Range(0, lootPool.Count)];
            if (candidate == null)
            {
                continue;
            }

            item = candidate;
            break;
        }

        int minCost = Mathf.Max(0, Mathf.Min(randomCostRange.x, randomCostRange.y));
        int maxCostInclusive = Mathf.Max(randomCostRange.x, randomCostRange.y);
        cost = Random.Range(minCost, maxCostInclusive + 1);
        sold = false;
        RefreshPrompt();
    }

    private bool TryPurchase(GameObject buyer)
    {
        if (sold || item == null || buyer == null)
        {
            return false;
        }

        var wallet = buyer.GetComponentInParent<PlayerWallet>();
        if (wallet == null || !wallet.TrySpend(cost))
        {
            return false;
        }

        var itemManager = buyer.GetComponentInParent<ItemManager>();
        if (itemManager != null)
        {
            itemManager.AddItem(item, buyer);
        }

        sold = true;
        RefreshPrompt();
        return true;
    }
    #endregion
}
