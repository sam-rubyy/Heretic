using System.Collections;
using UnityEngine;
using TMPro;

[DisallowMultipleComponent]
public class ItemPickupPopup : MonoBehaviour
{
    #region Fields
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private bool startHidden = true;
    private Coroutine displayRoutine;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (titleText == null || subtitleText == null)
        {
            var texts = GetComponentsInChildren<TMP_Text>();
            if (texts != null)
            {
                if (texts.Length > 0 && titleText == null)
                {
                    titleText = texts[0];
                }
                if (texts.Length > 1 && subtitleText == null)
                {
                    subtitleText = texts[1];
                }
            }
        }

        if (startHidden && canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }

    private void OnEnable()
    {
        GameplayEvents.OnItemCollected += HandleItemCollected;
    }

    private void OnDisable()
    {
        GameplayEvents.OnItemCollected -= HandleItemCollected;
    }
    #endregion

    #region Private Methods
    private void HandleItemCollected(ItemBase item, GameObject collector)
    {
        if (item == null)
        {
            return;
        }

        if (displayRoutine != null)
        {
            StopCoroutine(displayRoutine);
        }

        displayRoutine = StartCoroutine(DisplayRoutine(item));
    }

    private IEnumerator DisplayRoutine(ItemBase item)
    {
        SetText(item.DisplayName, item.Description);
        yield return FadeTo(1f, fadeDuration);

        yield return new WaitForSeconds(displayDuration);

        yield return FadeTo(0f, fadeDuration);
        displayRoutine = null;
    }

    private void SetText(string title, string subtitle)
    {
        if (titleText != null)
        {
            titleText.text = string.IsNullOrWhiteSpace(title) ? "Item Acquired" : title;
        }

        if (subtitleText != null)
        {
            subtitleText.text = subtitle ?? string.Empty;
        }
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        if (canvasGroup == null || duration <= 0f)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = targetAlpha;
            }
            yield break;
        }

        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }
    #endregion
}
