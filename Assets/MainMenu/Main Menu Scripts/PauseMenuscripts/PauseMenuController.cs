using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenuController : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private UnityEngine.InputSystem.PlayerInput playerInput;


    [Header("Flow")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private bool disablePlayerInputWhilePaused = true;

    [Header("Timing")]
    [SerializeField] private float fadeDuration = 0.25f;

    [SerializeField] private GameObject hudRoot;

    [Header("Audio")]
    [SerializeField] private AudioSource pauseMusicSource;     
    [SerializeField] private AudioClip pauseMusicClip;

    [SerializeField] private AudioSource sfxSource;            
    [SerializeField] private AudioClip openPauseSfx;          
    [SerializeField] private AudioClip closePauseSfx;          


    private bool isOpen = false;

    void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }


        if (playerInput == null)
        {
#if UNITY_2023_1_OR_NEWER
            playerInput = Object.FindAnyObjectByType<UnityEngine.InputSystem.PlayerInput>();
#else
            playerInput = FindObjectOfType<UnityEngine.InputSystem.PlayerInput>();
#endif
        }
    }

    void Update()
    {
        
        if (InventoryScreenController.EscRecentlyConsumed)
            return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {

            if (!isOpen && Time.timeScale == 0f) return;

            if (isOpen) Resume();
            else Pause();
        }
    }

    private void PlayToggleSfx(AudioClip clip)
    {
        if (!sfxSource || !clip) return;
        sfxSource.ignoreListenerPause = true;  
        sfxSource.PlayOneShot(clip);
    }

    private void StartPauseMusic()
    {
        if (!pauseMusicSource || !pauseMusicClip) return;
        pauseMusicSource.ignoreListenerPause = true;  
        pauseMusicSource.clip = pauseMusicClip;
        if (!pauseMusicSource.isPlaying) pauseMusicSource.Play();
    }

    private void StopPauseMusic()
    {
        if (pauseMusicSource && pauseMusicSource.isPlaying)
            pauseMusicSource.Stop();
    }

    public void Pause()
    {
        if (isOpen) return;
        isOpen = true;

        if (hudRoot) hudRoot.SetActive(false);

        PlayToggleSfx(openPauseSfx);
        StartPauseMusic();

        StartCoroutine(FadeCanvas(1f));
        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (disablePlayerInputWhilePaused && playerInput != null)
            playerInput.enabled = false;
    }

    public void Resume()
    {
        if (!isOpen) return;
        isOpen = false;

        Time.timeScale = 1f;

        PlayToggleSfx(closePauseSfx);
        StopPauseMusic();

        if (hudRoot) hudRoot.SetActive(true);

        if (disablePlayerInputWhilePaused && playerInput != null)
            playerInput.enabled = true;

        StartCoroutine(FadeCanvas(0f));


        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }


    public void MainMenu()
    {
        Time.timeScale = 1f;
        if (disablePlayerInputWhilePaused && playerInput != null)
            playerInput.enabled = true;

        if (!string.IsNullOrEmpty(mainMenuSceneName))
            SceneManager.LoadScene(mainMenuSceneName);
        else
            Debug.LogWarning("PauseMenuController: Main menu scene name not set.");
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        if (disablePlayerInputWhilePaused && playerInput != null)
            playerInput.enabled = true;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private IEnumerator FadeCanvas(float target)
    {
        if (canvasGroup == null)
            yield break;

        float start = canvasGroup.alpha;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = target;
        bool on = target > 0.5f;
        canvasGroup.interactable = on;
        canvasGroup.blocksRaycasts = on;
    }


}


