using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class FireTrailEffect : MonoBehaviour
{
    #region Fields
    [SerializeField] private float radius = 0.6f;
    [SerializeField] private float lifetime = 1.5f;
    [SerializeField] private float tickInterval = 0.25f;
    [SerializeField] private float damagePerTick = 1f;
    [SerializeField] private BulletOwner owner = BulletOwner.Neutral;
    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite sprite;
    [SerializeField] private Color color = new Color(1f, 0.55f, 0.2f, 0.4f);
    [SerializeField] private string sortingLayer = "Effects";
    [SerializeField] private int sortingOrder = 0;
    #endregion

    #region Unity Methods
    private void Start()
    {
        SetupVisual();
        StartCoroutine(DamageRoutine());
    }
    #endregion

    #region Public Methods
    public void Initialize(float damage, float tick, float life, float effectRadius, BulletOwner effectOwner)
    {
        damagePerTick = Mathf.Max(0f, damage);
        tickInterval = Mathf.Max(0.05f, tick);
        lifetime = Mathf.Max(0.1f, life);
        radius = Mathf.Max(0.05f, effectRadius);
        owner = effectOwner;

        SetupVisual();
    }
    #endregion

    #region Private Methods
    private void SetupVisual()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
        }

        if (sprite != null || spriteRenderer.sprite == null)
        {
            spriteRenderer.sprite = sprite;
        }
        spriteRenderer.color = color;
        if (!string.IsNullOrWhiteSpace(sortingLayer))
        {
            spriteRenderer.sortingLayerName = sortingLayer;
        }
        spriteRenderer.sortingOrder = sortingOrder;

        // Match visual diameter to damage radius (assumes sprite is 1 unit wide in world space).
        float diameter = radius * 2f;
        transform.localScale = new Vector3(diameter, diameter, 1f);
    }

    private IEnumerator DamageRoutine()
    {
        float elapsed = 0f;
        float tickTimer = 0f;

        while (elapsed < lifetime)
        {
            float delta = Time.deltaTime;
            elapsed += delta;
            tickTimer += delta;

            if (tickTimer >= tickInterval)
            {
                tickTimer -= tickInterval;
                ApplyDamage();
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    private void ApplyDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        for (int i = 0; i < hits.Length; i++)
        {
            var hit = hits[i];
            if (hit == null)
            {
                continue;
            }

            if (owner == BulletOwner.Player)
            {
                var enemyHealth = hit.GetComponentInParent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damagePerTick);
                }
            }
            else if (owner == BulletOwner.Enemy)
            {
                var playerHealth = hit.GetComponentInParent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(Mathf.RoundToInt(damagePerTick));
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
    #endregion
}
