using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HealthDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private RectTransform heartsParent;
    [SerializeField] private Image heartPrefab;

    [Header("Sprites")]
    [SerializeField] private Sprite emptyHeart;
    [SerializeField] private Sprite fullHeart;

    [Header("Layout")]
    [SerializeField, Min(1)] private int heartsPerRow = 10;
    [SerializeField] private Vector2 heartSpacing = new Vector2(4f, 4f);

    private readonly List<Image> hearts = new();
    private GridLayoutGroup layoutGroup;
    private int cachedMaxHealth = -1;
    private int cachedHealth = -1;
    private bool configurationValid = true;

    private void Awake()
    {
        if (heartsParent == null)
            heartsParent = GetComponent<RectTransform>();

        if (heartPrefab == null && heartsParent != null && heartsParent.childCount > 0)
            heartPrefab = heartsParent.GetChild(0).GetComponent<Image>();

        if (heartsParent != null)
        {
            layoutGroup = heartsParent.GetComponent<GridLayoutGroup>();
            if (layoutGroup == null)
                layoutGroup = heartsParent.gameObject.AddComponent<GridLayoutGroup>();
        }
        else
        {
            Debug.LogError($"{nameof(HealthDisplay)} requires a RectTransform parent for hearts.", this);
            configurationValid = false;
            return;
        }

        bool skipTemplate = heartPrefab != null && heartsParent != null && heartPrefab.transform.IsChildOf(heartsParent);
        if (skipTemplate)
            heartPrefab.gameObject.SetActive(false);

        CacheExistingHearts(skipTemplate);
        ConfigureLayout();
    }

    private void OnEnable()
    {
        ForceRefresh();
    }

    private void Update()
    {
        if (!configurationValid)
            return;

        if (playerHealth == null)
            return;

        if (playerHealth.MaxHealth != cachedMaxHealth)
        {
            RebuildHearts(playerHealth.MaxHealth);
            cachedMaxHealth = playerHealth.MaxHealth;
        }

        if (playerHealth.Health != cachedHealth)
        {
            UpdateHeartSprites(playerHealth.Health);
            cachedHealth = playerHealth.Health;
        }
    }

    private void ForceRefresh()
    {
        if (!configurationValid)
            return;

        cachedMaxHealth = -1;
        cachedHealth = -1;

        if (playerHealth == null)
            return;

        RebuildHearts(playerHealth.MaxHealth);
        cachedMaxHealth = playerHealth.MaxHealth;

        UpdateHeartSprites(playerHealth.Health);
        cachedHealth = playerHealth.Health;
    }

    private void ConfigureLayout()
    {
        if (layoutGroup == null)
            return;

        layoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layoutGroup.constraintCount = Mathf.Max(1, heartsPerRow);
        layoutGroup.spacing = heartSpacing;
        layoutGroup.childAlignment = TextAnchor.UpperLeft;

        Vector2 cellSize = layoutGroup.cellSize;
        if (heartPrefab != null)
        {
            cellSize = heartPrefab.rectTransform.sizeDelta;
        }
        else if (hearts.Count > 0 && hearts[0] != null)
        {
            cellSize = hearts[0].rectTransform.sizeDelta;
        }

        if (cellSize == Vector2.zero)
            cellSize = new Vector2(32f, 32f);

        layoutGroup.cellSize = cellSize;
    }

    private void CacheExistingHearts(bool skipTemplate)
    {
        hearts.Clear();

        if (heartsParent == null)
            return;

        for (int i = 0; i < heartsParent.childCount; i++)
        {
            var image = heartsParent.GetChild(i).GetComponent<Image>();
            if (image == null)
                continue;

            if (skipTemplate && image == heartPrefab)
                continue;

            hearts.Add(image);
        }
    }

    private void RebuildHearts(int targetCount)
    {
        if (targetCount < 0)
            targetCount = 0;

        ConfigureLayout();
        EnsureHeartCount(targetCount);
        UpdateHeartSprites(playerHealth != null ? playerHealth.Health : targetCount);
    }

    private void EnsureHeartCount(int targetCount)
    {
        if (!configurationValid || heartsParent == null)
        {
            return;
        }

        while (hearts.Count < targetCount)
            AddHeart();

        while (hearts.Count > targetCount)
            RemoveLastHeart();
    }

    private void AddHeart()
    {
        if (heartsParent == null)
            return;

        Image newHeart;
        if (heartPrefab != null)
        {
            newHeart = Instantiate(heartPrefab, heartsParent);
        }
        else
        {
            var go = new GameObject("Heart", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(heartsParent, false);
            newHeart = go.GetComponent<Image>();
        }

        newHeart.gameObject.SetActive(true);
        newHeart.sprite = emptyHeart != null ? emptyHeart : fullHeart;
        hearts.Add(newHeart);
    }

    private void RemoveLastHeart()
    {
        if (hearts.Count == 0)
            return;

        int lastIndex = hearts.Count - 1;
        Image lastHeart = hearts[lastIndex];
        hearts.RemoveAt(lastIndex);

        if (lastHeart == null)
            return;

        if (lastHeart == heartPrefab)
        {
            lastHeart.gameObject.SetActive(false);
            return;
        }

        Destroy(lastHeart.gameObject);
    }

    private void UpdateHeartSprites(int currentHealth)
    {
        for (int i = 0; i < hearts.Count; i++)
        {
            Image heart = hearts[i];
            if (heart == null)
                continue;

            heart.sprite = i < currentHealth ? fullHeart : emptyHeart;
        }
    }
}
