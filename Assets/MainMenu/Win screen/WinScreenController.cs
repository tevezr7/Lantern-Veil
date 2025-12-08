using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class WinScreenController : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private GameObject root;           // whole win screen
    [SerializeField] private CanvasGroup canvasGroup;   // for fading UI
    [SerializeField] private Image backgroundImage;     // full-screen bg image

    [Header("Timing")]
    [SerializeField] private float winDelay = 1.0f;     // delay BEFORE win screen starts
    [SerializeField] private float backgroundFadeDuration = 0.8f;
    [SerializeField] private float uiFadeDuration = 0.6f;

    [Header("Music")]
    [SerializeField] private AudioSource musicSource;   // dedicated AudioSource on this canvas
    [SerializeField] private AudioClip victoryLoopClip;
    [SerializeField][Range(0f, 1f)] private float victoryVolume = 0.7f;

    [Header("Stats UI")]
    [SerializeField] private TMP_Text timeSurvivedText;
    [SerializeField] private TMP_Text potionsUsedText;   
                                                         


    [Header("Navigation")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    bool isShowing = false;

    void Awake()
    {
        if (!root) root = gameObject;
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();

        // Start INVISIBLE but ACTIVE
        if (canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (backgroundImage != null)
        {
            var c = backgroundImage.color;
            c.a = 0f;
            backgroundImage.color = c;
        }

        // ❌ DO NOT do root.SetActive(false) here
        // Root must stay active so coroutines can run
    }

    void UnlockCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ShowWinScreen()
    {
        if (isShowing) return;
        isShowing = true;

        // Start the full win sequence (delay -> stats -> fade)
        StartCoroutine(WinSequence());
    }

    System.Collections.IEnumerator WinSequence()
    {
        // Capture stats BEFORE freezing time
        float timeSurvived = Time.timeSinceLevelLoad;
        if (GameSessionStats.Instance != null)
        {
            GameSessionStats.Instance.timeSurvived = timeSurvived;
        }

        // Small delay so the kill doesn't hard-cut straight into UI
        yield return new WaitForSeconds(winDelay);

        UnlockCursor();

        // Pause all in-game audio, but we'll play music with ignoreListenerPause
        AudioListener.pause = true;

        // Start victory loop
        if (musicSource != null && victoryLoopClip != null)
        {
            musicSource.ignoreListenerPause = true;
            musicSource.clip = victoryLoopClip;
            musicSource.volume = victoryVolume;
            musicSource.Play();
        }

        // Fill in stat texts
        PopulateStatsUI(timeSurvived);

        // Just in case it was disabled in editor, ensure active
        if (!root.activeSelf)
            root.SetActive(true);

        yield return StartCoroutine(FadeInSequence());
    }

    void PopulateStatsUI(float timeSurvived)
    {
        GameSessionStats stats = GameSessionStats.Instance;

        // Time
        if (timeSurvivedText != null)
            timeSurvivedText.text = "Time Survived: " + FormatTime(timeSurvived);

        if (stats != null)
        {
            if (potionsUsedText != null)
                potionsUsedText.text = "Potions Used: " + stats.TotalPotionsUsed;
        }
        else
        {
            if (potionsUsedText != null)
                potionsUsedText.text = "Potions Used: 0";
        }
    }

    string FormatTime(float timeSeconds)
    {
        int totalSeconds = Mathf.FloorToInt(timeSeconds);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return $"{minutes:00}:{seconds:00}";
    }

    System.Collections.IEnumerator FadeInSequence()
    {
        float tBg = 0f;
        float tUi = 0f;

        if (canvasGroup) canvasGroup.alpha = 0f;

        // We can freeze gameplay time now; we use unscaledDeltaTime
        Time.timeScale = 0f;

        while (tBg < backgroundFadeDuration || tUi < uiFadeDuration)
        {
            float dt = Time.unscaledDeltaTime;

            // Background fade
            if (backgroundImage != null && tBg < backgroundFadeDuration)
            {
                tBg += dt;
                float a = Mathf.Clamp01(tBg / backgroundFadeDuration);
                var c = backgroundImage.color;
                c.a = a;
                backgroundImage.color = c;
            }

            // UI fade
            if (canvasGroup != null && tUi < uiFadeDuration)
            {
                tUi += dt;
                float a = Mathf.Clamp01(tUi / uiFadeDuration);
                canvasGroup.alpha = a;
            }

            yield return null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    // --- Buttons ---

    public void PlayAgain()
    {
        AudioListener.pause = false;
        Time.timeScale = 1f;

        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }

    public void MainMenu()
    {
        AudioListener.pause = false;
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(mainMenuSceneName))
            SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        AudioListener.pause = false;
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
