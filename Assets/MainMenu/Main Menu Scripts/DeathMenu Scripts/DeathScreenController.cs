using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScreenController : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip deathSfx;

    [Header("Links")]
    [SerializeField] private CanvasGroup canvasGroup;   
    [SerializeField] private GameObject root;          
    [SerializeField] private string mainMenuSceneName = "MainMenu"; 

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.6f;

    bool isShowing = false;

    void Awake()
    {
        if (!root) root = gameObject;
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private void UnlockCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void LockCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }



    public void ShowDeathScreen()
    {
        if (isShowing) return;
        isShowing = true;

        UnlockCursor();

        AudioListener.pause = true;

        if (sfxSource)
        {
            sfxSource.ignoreListenerPause = true;  
            sfxSource.PlayOneShot(deathSfx);
        }

#if UNITY_2023_1_OR_NEWER
        var low = Object.FindAnyObjectByType<LowHealthFX>(FindObjectsInactive.Include);
#else
    var low = FindObjectOfType<LowHealthFX>();
#endif
        if (low != null)
            low.ForceClear();

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
        StartCoroutine(FadeIn());
    }



    System.Collections.IEnumerator FadeIn()
    {
        root.SetActive(true);

        float t = 0f;
        if (canvasGroup) canvasGroup.alpha = 0f;

        
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / fadeDuration);
            if (canvasGroup) canvasGroup.alpha = a;
            yield return null;
        }

      
        Time.timeScale = 0f;

        if (canvasGroup)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }


    public void Retry()
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
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
