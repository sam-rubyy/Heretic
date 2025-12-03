using System;
using System.Collections;
using System.Collections.Generic;
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
    private Color[] baseColors;
    private readonly Dictionary<string, Coroutine> activeEffects = new Dictionary<string, Coroutine>();
    private readonly Dictionary<string, float> speedFactors = new Dictionary<string, float>();
    private float moveSpeedMultiplier = 1f;
    private EnemyBase owner;
    #endregion

    #region Events
    public event Action<EnemyBase> Died;
    #endregion

    #region Properties
    public float MoveSpeedMultiplier => moveSpeedMultiplier;
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

        CacheBaseColors();
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
        if (effectParams.duration <= 0f || effectParams.intensity <= 0f || string.IsNullOrWhiteSpace(effectParams.effectId))
        {
            return;
        }

        string id = effectParams.effectId.Trim().ToLowerInvariant();

        // Restart effect if already active.
        if (activeEffects.TryGetValue(id, out var routine) && routine != null)
        {
            StopCoroutine(routine);
        }

        Coroutine newRoutine = null;
        switch (id)
        {
            case "burn":
            case "burning":
                newRoutine = StartCoroutine(BurningEffect(effectParams));
                break;
            case "poison":
                newRoutine = StartCoroutine(PoisonEffect(effectParams));
                break;
            case "regeneration":
            case "regen":
                newRoutine = StartCoroutine(RegenerationEffect(effectParams));
                break;
            case "slow":
            case "slowed":
                newRoutine = StartCoroutine(SlowedEffect(effectParams, slow: true));
                break;
            case "haste":
                newRoutine = StartCoroutine(SlowedEffect(effectParams, slow: false));
                break;
            case "frozen":
            case "freeze":
                newRoutine = StartCoroutine(FrozenEffect(effectParams));
                break;
            default:
                newRoutine = StartCoroutine(GenericDot(effectParams));
                break;
        }

        activeEffects[id] = newRoutine;
    }
    #endregion

    #region Private Methods
    private void PlayHitFlash()
    {
        if (renderers == null || renderers.Length == 0)
        {
            return;
        }

        if (flashRoutine != null)
        {
            ResetToBaseColors();
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }

        if (baseColors == null || baseColors.Length != renderers.Length)
        {
            CacheBaseColors();
        }

        flashRoutine = StartCoroutine(HitFlashRoutine());
    }

    private void CacheBaseColors()
    {
        if (renderers == null)
        {
            return;
        }

        baseColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            baseColors[i] = renderers[i].color;
        }
    }

    private void ResetToBaseColors()
    {
        if (renderers == null || baseColors == null || renderers.Length != baseColors.Length)
        {
            return;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].color = baseColors[i];
            }
        }
    }

    private IEnumerator HitFlashRoutine()
    {
        ResetToBaseColors();

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].color = hitFlashColor;
            }
        }

        yield return new WaitForSecondsRealtime(hitFlashDuration);

        ResetToBaseColors();

        flashRoutine = null;
    }

    private void OnDisable()
    {
        ResetToBaseColors();
        foreach (var kvp in activeEffects)
        {
            if (kvp.Value != null)
            {
                StopCoroutine(kvp.Value);
            }
        }
        activeEffects.Clear();
        speedFactors.Clear();
        moveSpeedMultiplier = 1f;
    }

    private IEnumerator GenericDot(StatusEffectParams effectParams)
    {
        string id = effectParams.effectId.Trim().ToLowerInvariant();
        float elapsed = 0f;
        float tick = 1f;
        while (elapsed < effectParams.duration && currentHealth > 0)
        {
            TakeDamage(effectParams.intensity);
            elapsed += tick;
            yield return new WaitForSecondsRealtime(tick);
        }

        activeEffects.Remove(id);
    }

    private IEnumerator BurningEffect(StatusEffectParams effectParams)
    {
        string id = effectParams.effectId.Trim().ToLowerInvariant();
        float elapsed = 0f;
        float tick = 0.5f;
        while (elapsed < effectParams.duration && currentHealth > 0)
        {
            TakeDamage(effectParams.intensity);
            elapsed += tick;
            yield return new WaitForSecondsRealtime(tick);
        }

        activeEffects.Remove(id);
    }

    private IEnumerator PoisonEffect(StatusEffectParams effectParams)
    {
        string id = effectParams.effectId.Trim().ToLowerInvariant();
        float elapsed = 0f;
        float tick = 0.75f;
        while (elapsed < effectParams.duration && currentHealth > 0)
        {
            TakeDamage(effectParams.intensity * 0.75f + 0.25f);
            elapsed += tick;
            yield return new WaitForSecondsRealtime(tick);
        }

        activeEffects.Remove(id);
    }

    private IEnumerator RegenerationEffect(StatusEffectParams effectParams)
    {
        string id = effectParams.effectId.Trim().ToLowerInvariant();
        float elapsed = 0f;
        float tick = 1f;
        while (elapsed < effectParams.duration)
        {
            Heal(Mathf.RoundToInt(effectParams.intensity));
            elapsed += tick;
            yield return new WaitForSecondsRealtime(tick);
        }

        activeEffects.Remove(id);
    }

    private IEnumerator SlowedEffect(StatusEffectParams effectParams, bool slow)
    {
        string id = effectParams.effectId.Trim().ToLowerInvariant();
        string key = slow ? "slow" : "haste";
        float factor = slow
            ? Mathf.Clamp01(1f - effectParams.intensity)
            : Mathf.Max(0.1f, 1f + effectParams.intensity);

        AddSpeedFactor(key, factor);
        yield return new WaitForSecondsRealtime(effectParams.duration);
        RemoveSpeedFactor(key);
        activeEffects.Remove(id);
    }

    private IEnumerator FrozenEffect(StatusEffectParams effectParams)
    {
        string id = effectParams.effectId.Trim().ToLowerInvariant();
        const string key = "frozen";
        AddSpeedFactor(key, 0f);
        yield return new WaitForSecondsRealtime(effectParams.duration);
        RemoveSpeedFactor(key);
        activeEffects.Remove(id);
    }

    private void AddSpeedFactor(string key, float factor)
    {
        speedFactors[key] = factor;
        RecalculateSpeedMultiplier();
    }

    private void RemoveSpeedFactor(string key)
    {
        if (speedFactors.ContainsKey(key))
        {
            speedFactors.Remove(key);
            RecalculateSpeedMultiplier();
        }
    }

    private void RecalculateSpeedMultiplier()
    {
        float value = 1f;
        foreach (var factor in speedFactors.Values)
        {
            value *= factor;
        }

        moveSpeedMultiplier = Mathf.Clamp(value, 0f, 3f);
    }
    #endregion
}
