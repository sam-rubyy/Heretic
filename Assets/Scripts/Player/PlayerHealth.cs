using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerHealth : MonoBehaviour
{
    #region Fields
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private int currentHealth;
    [Header("Invulnerability")]
    [SerializeField] private float invulnerabilityDuration = 0.5f;
    [SerializeField] private float blinkInterval = 0.1f;
    [Header("Feedback")]
    [SerializeField] private Color hitFlashColor = Color.red;
    [SerializeField] private float hitFlashDuration = 0.1f;
    [SerializeField] private SpriteRenderer[] renderers;

    public int Health => currentHealth;
    public int MaxHealth => maxHealth;


    private Rigidbody2D body;
    private bool isDead;
    private Coroutine invulRoutine;
    private Coroutine flashRoutine;
    private bool isInvulnerable;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        currentHealth = Mathf.Max(0, maxHealth);

        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<SpriteRenderer>();

        body = GetComponent<Rigidbody2D>();
    }
    #endregion

    #region Public Methods
    public void TakeDamage(int amount)
    {
        TakeDamage(amount, transform.position, 0f);
    }

    public void TakeDamage(int amount, Vector2 sourcePosition, float knockbackForce)
    {
        if (amount <= 0 || currentHealth <= 0 || isInvulnerable)
        {
            return;
        }

        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
        PlayHitFlash();
        ApplyKnockback(sourcePosition, knockbackForce);
        StartInvulnerability();

        if (currentHealth <= 0)
        {
            HandleDeath();
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
        isDead = false;
    }
    #endregion

    #region Private Methods
    private void HandleDeath()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        GameplayEvents.RaisePlayerDied(this);
    }

    private void ApplyKnockback(Vector2 sourcePosition, float force)
    {
        if (force <= 0f)
            return;

        Vector2 direction = ((Vector2)transform.position - sourcePosition);
        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector2.right;

        if (TryGetComponent<IKnockbackReceiver>(out var receiver))
        {
            receiver.ApplyKnockback(direction.normalized, force);
        }
        else if (body != null)
        {
            body.AddForce(direction.normalized * force, ForceMode2D.Impulse);
        }
    }

    private void StartInvulnerability()
    {
        if (invulnerabilityDuration <= 0f)
            return;

        if (invulRoutine != null)
            StopCoroutine(invulRoutine);

        invulRoutine = StartCoroutine(InvulnerabilityRoutine());
    }

    private IEnumerator InvulnerabilityRoutine()
    {
        isInvulnerable = true;

        float elapsed = 0f;
        bool dimmed = false;

        while (elapsed < invulnerabilityDuration)
        {
            SetRendererAlpha(dimmed ? 0.25f : 1f);
            dimmed = !dimmed;

            float wait = Mathf.Max(0.01f, blinkInterval);
            elapsed += wait;
            yield return new WaitForSeconds(wait);
        }

        SetRendererAlpha(1f);
        isInvulnerable = false;
        invulRoutine = null;
    }

    private void SetRendererAlpha(float alpha)
    {
        if (renderers == null)
            return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null)
                continue;

            Color c = renderers[i].color;
            c.a = alpha;
            renderers[i].color = c;
        }
    }

    private void PlayHitFlash()
    {
        if (renderers == null || renderers.Length == 0)
            return;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(HitFlashRoutine());
    }

    private IEnumerator HitFlashRoutine()
    {
        var originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = renderers[i].color;
            renderers[i].color = hitFlashColor;
        }

        yield return new WaitForSeconds(hitFlashDuration);

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                renderers[i].color = originalColors[i];
        }

        flashRoutine = null;
    }
    #endregion
}
