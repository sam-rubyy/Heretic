using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemyHealth : MonoBehaviour
{
    #region Fields
    [SerializeField] private float maxHealth = 15;
    [SerializeField] private float currentHealth;
    [Header("Feedback")]
    [SerializeField] private Color hitFlashColor = Color.red;
    [SerializeField] private float hitFlashDuration = 0.1f;
    [SerializeField] private SpriteRenderer[] renderers;
    [SerializeField] private Rigidbody2D body;
    private Coroutine flashRoutine;
    private EnemyBase owner;
    #endregion

    #region Events
    public event Action<EnemyBase> Died;
    #endregion
    

    #region Unity Methods
    private void Awake()
    {
        owner = GetComponent<EnemyBase>();
        if (body == null)
        {
            body = GetComponent<Rigidbody2D>();
        }

        if (renderers == null || renderers.Length == 0)
        {
            renderers = GetComponentsInChildren<SpriteRenderer>();
        }
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
        PlayHitFlash();

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

    public void ApplyKnockback(Vector2 sourcePosition, float force)
    {
        if (force <= 0f)
        {
            return;
        }

        Vector2 direction = ((Vector2)transform.position - sourcePosition);
        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = Vector2.right;
        }

        if (TryGetComponent<IKnockbackReceiver>(out var receiver))
        {
            receiver.ApplyKnockback(direction.normalized, force);
        }
        else if (body != null)
        {
            body.AddForce(direction.normalized * force, ForceMode2D.Impulse);
        }
    }

    public void ApplyStatusEffect(StatusEffectParams effectParams)
    {
        // Future effects can stack; for now just run each independently.
        StartCoroutine(HandleStatusEffect(effectParams));
    }
    #endregion

    #region Private Methods
    private IEnumerator HandleStatusEffect(StatusEffectParams effectParams)
    {
        if (effectParams.duration <= 0f || effectParams.intensity <= 0f)
        {
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < effectParams.duration && currentHealth > 0)
        {
            TakeDamage(effectParams.intensity);
            elapsed += 1f;
            yield return new WaitForSeconds(1f);
        }
    }

    private void PlayHitFlash()
    {
        if (renderers == null || renderers.Length == 0)
        {
            return;
        }

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

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
            {
                renderers[i].color = originalColors[i];
            }
        }

        flashRoutine = null;
    }
    #endregion
}
