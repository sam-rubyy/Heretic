using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class MoneyPickup : MonoBehaviour
{
    #region Fields
    [SerializeField, Min(1)] private int amount = 1;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite overrideSprite;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (spriteRenderer != null && overrideSprite != null)
        {
            spriteRenderer.sprite = overrideSprite;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null)
        {
            return;
        }

        var wallet = other.GetComponentInParent<PlayerWallet>();
        if (wallet == null)
        {
            return;
        }

        wallet.AddMoney(amount);
        Destroy(gameObject);
    }
    #endregion

    #region Public Methods
    public void SetAmount(int value)
    {
        amount = Mathf.Max(1, value);
    }
    #endregion
}
