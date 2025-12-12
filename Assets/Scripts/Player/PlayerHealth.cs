using System.Collections;
using System.Collections.Generic;
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
    [Header("Game Over")]
    [SerializeField, Tooltip("Shown when the player dies.")] private GameObject gameOverScreen;
    [SerializeField, Tooltip("Pause the game when showing the game over screen.")] private bool pauseOnGameOver = true;
    [SerializeField, Tooltip("Disable player control components on death.")] private bool disableControlOnDeath = true;

    public int Health => currentHealth;
    public int MaxHealth => maxHealth;

    // Status effects
    private readonly Dictionary<string, Coroutine> activeEffects = new Dictionary<string, Coroutine>();
    private readonly Dictionary<string, float> speedFactors = new Dictionary<string, float>();
    private float moveSpeedMultiplier = 1f;

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

    public float GetMoveSpeedMultiplier()
    {
        return moveSpeedMultiplier;
    }

    public void ApplyStatusEffect(StatusEffectParams effectParams)
    {
        if (effectParams.duration <= 0f || effectParams.intensity <= 0f || string.IsNullOrWhiteSpace(effectParams.effectId))
        {
            return;
        }

        string id = effectParams.effectId.Trim().ToLowerInvariant();

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
    private void HandleDeath()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        GameplayEvents.RaisePlayerDied(this);

        TriggerGameOver();
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

    private void OnDisable()
    {
        isInvulnerable = false;
        invulRoutine = null;
        flashRoutine = null;

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

    private void TriggerGameOver()
    {
        if (disableControlOnDeath)
        {
            var movement = GetComponent<PlayerMovement>();
            if (movement != null)
            {
                movement.enabled = false;
            }
        }

        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
        }

        if (pauseOnGameOver)
        {
            Time.timeScale = 0f;
        }
    }

    private IEnumerator GenericDot(StatusEffectParams effectParams)
    {
        string id = effectParams.effectId.Trim().ToLowerInvariant();
        float elapsed = 0f;
        float tick = 1f;
        while (elapsed < effectParams.duration && currentHealth > 0)
        {
            TakeDamage(Mathf.RoundToInt(effectParams.intensity));
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
            TakeDamage(Mathf.RoundToInt(effectParams.intensity));
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
            TakeDamage(Mathf.RoundToInt(effectParams.intensity * 0.75f + 0.25f));
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
