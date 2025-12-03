using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


// Hi i exist :)

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    [SerializeField] Selectable firstSelected;
    [SerializeField] Color selectionColor = new Color(1f, 0.9f, 0.4f, 1f);
    [SerializeField] List<Selectable> navigationOrder = new List<Selectable>();
    [Header("Buttons (optional wiring)")]
    [SerializeField] Button resumeButton;
    [SerializeField] Button quitButton;
    [SerializeField] Button homeButton;
    [SerializeField] string homeSceneName = "";
    [SerializeField] int homeSceneId = 0;

    EventSystem eventSystem;
    InputAction pauseAction;
    readonly List<Selectable> menuSelectables = new List<Selectable>();
    bool isPaused;

    public void Pause()
    {
        if (isPaused)
        {
            return;
        }

        ShowPauseMenu();
        Time.timeScale = 0f;
        isPaused = true;

        ConfigureNavigation();
        FocusFirstSelectable();
    }

    public void Resume()
    {
        if (!isPaused)
        {
            return;
        }

        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        if (eventSystem != null && eventSystem.currentSelectedGameObject != null)
        {
            eventSystem.SetSelectedGameObject(null);
        }
    }

    void Awake()
    {
        eventSystem = EventSystem.current;

        pauseAction = new InputAction("Pause", binding: "<Keyboard>/escape");
        pauseAction.performed += OnPausePerformed;

        HidePauseButtonVisuals();
    }

    void OnEnable()
    {
        pauseAction?.Enable();

        HookButtons();
    }

    void OnDisable()
    {
        pauseAction?.Disable();

        UnhookButtons();
    }

    void OnDestroy()
    {
        if (pauseAction != null)
        {
            pauseAction.performed -= OnPausePerformed;
            pauseAction.Dispose();
        }

        UnhookButtons();
    }

    void OnPausePerformed(InputAction.CallbackContext context)
    {
        if (isPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    void HookButtons()
    {
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(Resume);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(Quit);
        }

        if (homeButton != null)
        {
            homeButton.onClick.AddListener(LoadHomeScene);
        }
    }

    void UnhookButtons()
    {
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveListener(Resume);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(Quit);
        }

        if (homeButton != null)
        {
            homeButton.onClick.RemoveListener(LoadHomeScene);
        }
    }

    void ConfigureNavigation()
    {
        menuSelectables.Clear();

        if (navigationOrder != null && navigationOrder.Count > 0)
        {
            menuSelectables.AddRange(navigationOrder);
        }
        else if (pauseMenu != null)
        {
            pauseMenu.GetComponentsInChildren(true, menuSelectables);
        }

        if (menuSelectables.Count == 0)
        {
            return;
        }

        for (int i = 0; i < menuSelectables.Count; i++)
        {
            var current = menuSelectables[i];

            if (current == null)
            {
                continue;
            }

            var nav = current.navigation;
            nav.mode = Navigation.Mode.Explicit;

            var upIndex = (i - 1 + menuSelectables.Count) % menuSelectables.Count;
            var downIndex = (i + 1) % menuSelectables.Count;

            nav.selectOnUp = menuSelectables[upIndex];
            nav.selectOnDown = menuSelectables[downIndex];
            nav.selectOnLeft = menuSelectables[upIndex];
            nav.selectOnRight = menuSelectables[downIndex];

            current.navigation = nav;

            var colors = current.colors;
            colors.selectedColor = selectionColor;
            colors.highlightedColor = selectionColor;
            current.colors = colors;
        }

        if (firstSelected == null && menuSelectables.Count > 0)
        {
            firstSelected = menuSelectables[0];
        }
    }

    void FocusFirstSelectable()
    {
        if (eventSystem == null)
        {
            eventSystem = EventSystem.current;
        }

        if (eventSystem == null)
        {
            return;
        }

        GameObject target = null;
        Selectable targetSelectable = null;

        if (firstSelected != null)
        {
            target = firstSelected.gameObject;
            targetSelectable = firstSelected;
        }
        else if (menuSelectables.Count > 0)
        {
            targetSelectable = menuSelectables[0];
            target = targetSelectable?.gameObject;
        }

        eventSystem.SetSelectedGameObject(null);

        if (target != null)
        {
            eventSystem.SetSelectedGameObject(target);
            targetSelectable?.Select();
        }
    }

    void HidePauseButtonVisuals()
    {
        var button = GetComponent<Button>();

        if (button != null)
        {
            button.interactable = false;
            button.enabled = false;
        }

        var graphics = GetComponentsInChildren<Graphic>(true);

        foreach (var graphic in graphics)
        {
            graphic.raycastTarget = false;
            graphic.enabled = false;
        }
    }

    void ShowPauseMenu()
    {
        if (pauseMenu == null)
        {
            return;
        }

        pauseMenu.SetActive(true);

        var canvasGroup = pauseMenu.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        var rectTransform = pauseMenu.transform as RectTransform;
        if (rectTransform != null && rectTransform.localScale == Vector3.zero)
        {
            rectTransform.localScale = Vector3.one;
        }
    }

    public void Quit()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Backward-compatible: uses homeSceneName if set, otherwise the provided build index.
    public void Home(int sceneID)
    {
        if (!string.IsNullOrWhiteSpace(homeSceneName))
        {
            LoadScene(-1, homeSceneName);
            return;
        }

        LoadScene(sceneID, null);
    }

    // New OnClick-friendly overload: ignores parameters, uses configured homeSceneName or homeSceneId.
    public void Home()
    {
        LoadHomeScene();
    }

    public void HomeByName(string sceneName)
    {
        LoadScene(-1, sceneName);
    }

    public void LoadHomeScene()
    {
        if (!string.IsNullOrWhiteSpace(homeSceneName))
        {
            LoadScene(-1, homeSceneName);
        }
        else
        {
            LoadScene(homeSceneId, null);
        }
    }

    void LoadScene(int sceneID, string sceneName)
    {
        Time.timeScale = 1f;

        if (!string.IsNullOrWhiteSpace(sceneName))
        {
            SceneManager.LoadScene(sceneName);
            return;
        }

        if (sceneID >= 0)
        {
            SceneManager.LoadScene(sceneID);
        }
        else
        {
            Debug.LogWarning("PauseMenu: No home scene specified. Set homeSceneName or homeSceneId in the inspector.");
        }
    }
}
