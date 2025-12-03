using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ScreenTintController : MonoBehaviour
{
    #region Fields
    private static ScreenTintController instance;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image overlay;
    private Coroutine routine;
    private Color currentColor = Color.clear;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureOverlay();
    }
    #endregion

    #region Public Methods
    public static void Show(Color color, float targetAlpha, float fadeSeconds)
    {
        EnsureInstance();
        if (instance == null)
        {
            return;
        }

        instance.StartFade(color, Mathf.Clamp01(targetAlpha), Mathf.Max(0f, fadeSeconds));
    }

    public static void Hide(float fadeSeconds)
    {
        if (instance == null)
        {
            return;
        }

        instance.StartFade(instance.currentColor, 0f, Mathf.Max(0f, fadeSeconds));
    }
    #endregion

    #region Private Methods
    private static void EnsureInstance()
    {
        if (instance != null)
        {
            return;
        }

        var go = new GameObject("ScreenTintController");
        instance = go.AddComponent<ScreenTintController>();
    }

    private void EnsureOverlay()
    {
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
        }

        if (overlay == null)
        {
            var overlayGO = new GameObject("Overlay");
            overlayGO.transform.SetParent(canvas.transform, false);
            overlay = overlayGO.AddComponent<Image>();
            overlay.rectTransform.anchorMin = Vector2.zero;
            overlay.rectTransform.anchorMax = Vector2.one;
            overlay.rectTransform.offsetMin = Vector2.zero;
            overlay.rectTransform.offsetMax = Vector2.zero;
            overlay.raycastTarget = false;
            overlay.color = Color.clear;
        }
    }

    private void StartFade(Color targetColor, float targetAlpha, float fadeSeconds)
    {
        EnsureOverlay();

        targetColor.a = targetAlpha;
        if (routine != null)
        {
            StopCoroutine(routine);
        }

        routine = StartCoroutine(FadeRoutine(targetColor, fadeSeconds));
    }

    private IEnumerator FadeRoutine(Color targetColor, float fadeSeconds)
    {
        Color startColor = overlay.color;
        if (fadeSeconds <= 0f)
        {
            overlay.color = targetColor;
            currentColor = targetColor;
            routine = null;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < fadeSeconds)
        {
            float t = elapsed / fadeSeconds;
            overlay.color = Color.Lerp(startColor, targetColor, t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        overlay.color = targetColor;
        currentColor = targetColor;
        routine = null;
    }
    #endregion
}
