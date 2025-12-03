using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class BulletTimeRunner : MonoBehaviour
{
    #region Fields
    private static bool useUnscaledPlayerTime = false;
    private static float playerTimeOffset = 0f;
    private Coroutine routine;
    private float originalScale = 1f;
    private float originalFixedDeltaTime = 0.02f;
    private float appliedScale = 1f;
    private bool restoreFixedDeltaTime = false;
    private bool lastAffectPlayer = true;
    [Header("Visuals")]
    [SerializeField] private Color tintColor = new Color(0.75f, 0f, 0f, 0.35f);
    [SerializeField] private float tintFadeInSeconds = 0.1f;
    [SerializeField] private float tintFadeOutSeconds = 0.2f;
    #endregion

    #region Public Methods
    public static bool UseUnscaledPlayerTime => useUnscaledPlayerTime;

    public static float GetPlayerTime()
    {
        if (Time.timeScale <= 0f)
        {
            return Time.time + playerTimeOffset;
        }

        return (useUnscaledPlayerTime ? Time.unscaledTime : Time.time) + playerTimeOffset;
    }

    public static float GetPlayerDeltaTime()
    {
        if (Time.timeScale <= 0f)
        {
            return 0f;
        }

        return useUnscaledPlayerTime ? Time.unscaledDeltaTime : Time.deltaTime;
    }

    public static float GetPlayerFixedDeltaTime()
    {
        if (Time.timeScale <= 0f)
        {
            return 0f;
        }

        return useUnscaledPlayerTime ? Time.fixedUnscaledDeltaTime : Time.fixedDeltaTime;
    }

    public void Trigger(float scale, float durationSeconds, bool ignoreWhenPaused, bool affectPlayer)
    {
        if (durationSeconds <= 0f)
        {
            return;
        }

        if (ignoreWhenPaused && Time.timeScale <= 0.0001f)
        {
            return;
        }

        float clampedScale = Mathf.Clamp(scale, 0.01f, 1f);

        if (routine != null)
        {
            StopCoroutine(routine);
        }

        SetUseUnscaledPlayerTime(!affectPlayer);
        lastAffectPlayer = affectPlayer;
        routine = StartCoroutine(BulletTimeRoutine(clampedScale, durationSeconds, ignoreWhenPaused, affectPlayer));
    }
    #endregion

    #region Private Methods
    private IEnumerator BulletTimeRoutine(float scale, float durationSeconds, bool ignoreWhenPaused, bool affectPlayer)
    {
        originalScale = Time.timeScale;
        originalFixedDeltaTime = Time.fixedDeltaTime;
        appliedScale = scale;
        restoreFixedDeltaTime = true;

        Time.timeScale = scale;
        Time.fixedDeltaTime = originalFixedDeltaTime * scale;
        ScreenTintController.Show(tintColor, tintColor.a, tintFadeInSeconds);

        if (!affectPlayer)
        {
            // Counteract player slowdown by scaling their rigidbody velocities back up.
            var player = GameObject.FindWithTag("Player");
            if (player != null && player.TryGetComponent<Rigidbody2D>(out var rb))
            {
                rb.velocity /= scale;
                rb.angularVelocity /= scale;
            }
        }

        float elapsed = 0f;
        while (elapsed < durationSeconds)
        {
            if (ignoreWhenPaused && Time.timeScale <= 0.0001f)
            {
                break;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        RestoreIfOwned(affectPlayer);
        routine = null;
    }

    private void RestoreIfOwned(bool affectPlayer)
    {
        if (Mathf.Approximately(Time.timeScale, appliedScale))
        {
            Time.timeScale = originalScale;
        }

        if (restoreFixedDeltaTime && Mathf.Approximately(Time.fixedDeltaTime, originalFixedDeltaTime * appliedScale))
        {
            Time.fixedDeltaTime = originalFixedDeltaTime;
        }

        if (!affectPlayer)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null && player.TryGetComponent<Rigidbody2D>(out var rb))
            {
                rb.velocity *= appliedScale / Mathf.Max(0.0001f, Time.timeScale);
                rb.angularVelocity *= appliedScale / Mathf.Max(0.0001f, Time.timeScale);
            }
        }

        SetUseUnscaledPlayerTime(false);
        ScreenTintController.Hide(tintFadeOutSeconds);
    }

    private static void SetUseUnscaledPlayerTime(bool useUnscaled)
    {
        if (useUnscaled == useUnscaledPlayerTime)
        {
            return;
        }

        float currentTime = (useUnscaledPlayerTime ? Time.unscaledTime : Time.time) + playerTimeOffset;
        useUnscaledPlayerTime = useUnscaled;
        float baseTime = useUnscaledPlayerTime ? Time.unscaledTime : Time.time;
        playerTimeOffset = currentTime - baseTime;
    }

    private void OnDisable()
    {
        RestoreIfOwned(lastAffectPlayer);
    }
    #endregion
}
