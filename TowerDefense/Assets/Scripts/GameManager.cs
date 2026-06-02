using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    #region Singleton Setup
    public static GameManager Instance;

    #region Loading
    [Header("Loading Screen Settings")]
    public bool skipLoading;
    public Image fadeImage;
    public float speed = 1;
    private enum FadeState {idle, fadingIn, fadingOut}
    private FadeState fadeState = FadeState.fadingOut;
    private float fadeAlpha = 1;
    int nextSceneIndex;
    #endregion

    #region Pause Menu
    [Header("Pause Menu References")]
    [SerializeField] private CanvasGroup pauseMenu;

    #endregion

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if(Instance != this)
        {
            Destroy(this.gameObject);
        }
    }
    #endregion

    [Header("Panic Button Behaviour")]
    public PanicButtonBehavior panicButtonType = PanicButtonBehavior.Standard;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //ensures this sticks between levels
        DontDestroyOnLoad(gameObject);
        if(skipLoading)
        {
            fadeAlpha = 0;
            fadeState = FadeState.idle;
        }
    }

    private void Update()
    {
        if (fadeImage == null) return;

        if(Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }

        switch(fadeState)
        {
            case FadeState.idle:
                fadeImage.color = new(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 0);
                break;
            case FadeState.fadingIn:
                if (fadeAlpha > 1)
                {
                    SceneManager.LoadScene(nextSceneIndex);
                }
                fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, Mathf.Min(1, fadeAlpha));
                fadeAlpha += speed * Time.deltaTime;
                break;
            case FadeState.fadingOut:
                if (fadeAlpha < 0)
                {
                    fadeState = FadeState.idle;
                }
                fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, Mathf.Max(0, fadeAlpha));
                fadeAlpha -= speed * Time.deltaTime;
                break;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        fadeState = FadeState.fadingOut;
        RefreshManagerWorldReferences();
        //any other functions we might have to do would go here
    }

    public void GoToNewSceneAfterDelay(int delay, string sceneName)
    {
        StartCoroutine(DelayBeforeScene(delay, sceneName));
    }

    public void GoToNewSceneAfterDelay(int delay, int sceneIndex)
    {
        StartCoroutine(DelayBeforeScene(delay, sceneIndex));
    }

    private IEnumerator DelayBeforeScene(int delay, string sceneName)
    {
        yield return new WaitForSeconds(delay);
        GoToNewScene(sceneName);
    }

    private IEnumerator DelayBeforeScene(int delay, int sceneIndex)
    {
        yield return new WaitForSeconds(delay);
        GoToNewScene(sceneIndex);
    }

    public void GoToNewScene(string sceneName)
    {
        for(int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if(name.Equals(sceneName))
            {
                GoToNewScene(i);
                return;
            }
        }
        Debug.LogWarning($"Scene '{sceneName}' not found in Build Settings!");
    }

    bool menuAnimating = false;
    public bool PauseActive { get; private set; } = false;
    float timeScaleBeforePause;

    public void ReturnToMainMenu()
    {
        if(PauseActive)
        {
            StartCoroutine(ReturnToMainMenuCoroutine());
        }
        else
        {
            GoToNewScene("Main Menu");
        }
    }

    IEnumerator ReturnToMainMenuCoroutine()
    {
        yield return StartCoroutine(PauseMenuAnimationCoroutine(false));
        PauseActive = false;
        GoToNewScene("Main Menu");
    }

    public void TogglePauseMenu()
    {
        if(!menuAnimating && SceneManager.GetActiveScene().buildIndex > 0 && fadeState == FadeState.idle)
        {
            PauseActive = !PauseActive;
            TogglePauseMenu(PauseActive);
        }
    }

    public void TogglePauseMenu(bool enabled)
    {
        PauseActive = enabled;
        if (PauseActive)
        {
            timeScaleBeforePause = Time.timeScale;
        }
        StartCoroutine(PauseMenuAnimationCoroutine(PauseActive));

    }

    private IEnumerator PauseMenuAnimationCoroutine(bool enabled)
    {
        Debug.Log(enabled);
        menuAnimating = true;
        pauseMenu.interactable = false;
        pauseMenu.blocksRaycasts = false;
        pauseMenu.alpha = enabled ? 0 : 1;
        RectTransform pauseMenuTransform = pauseMenu.transform as RectTransform;
        float alpha = 0;
        Vector3 target = new (enabled ? 0 : 1920, 0, 0);
        while(alpha < 1)
        {
            pauseMenuTransform.anchoredPosition = Vector3.Slerp(pauseMenuTransform.anchoredPosition, target, alpha);
            alpha += 0.01666666666666666666666666666667f * 5;
            alpha = Mathf.Clamp01(alpha);
            pauseMenu.alpha = enabled ? alpha : (1 - alpha);
            Time.timeScale = enabled ? (timeScaleBeforePause * (1 - alpha)) : timeScaleBeforePause * alpha;
            yield return new WaitForSecondsRealtime(0.01666666666666666666666666666667f);
        }
        pauseMenu.interactable = enabled;
        pauseMenu.blocksRaycasts = enabled;
        pauseMenu.alpha = enabled ? 1 : 0;
        pauseMenuTransform.anchoredPosition = target;
        menuAnimating = false;
    }

    public void GoToNewScene(int sceneIndex)
    {
        fadeState = FadeState.fadingIn;
        nextSceneIndex = sceneIndex;
    }

    private void RefreshManagerWorldReferences()
    {
        //If the manager would need to reference things in the level, that refreshing of variable references would happen here
    }

}
