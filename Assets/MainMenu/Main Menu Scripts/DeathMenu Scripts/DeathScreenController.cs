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



    public void ShowDeathScreen()
    {
        if (isShowing) return;
        isShowing = true;

        if (!root.activeSelf) root.SetActive(true);

        if (sfxSource != null && deathSfx != null)
        {
            sfxSource.PlayOneShot(deathSfx);
        }

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
        Time.timeScale = 1f;
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }

    public void MainMenu()
    {
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
