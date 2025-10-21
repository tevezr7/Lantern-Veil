using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string gameSceneName = "Lantern-Veil"; 

    [Header("Panels")]
    [SerializeField] private GameObject mainButtonsGroup;   
    [SerializeField] private GameObject optionsPanel;       

    // --- Buttons ---
    public void PlayGame()
    {
        if (SceneFader.I != null)
            SceneFader.I.FadeToScene(gameSceneName); 
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(gameSceneName);
    }


    public void OpenOptions()
    {
        if (mainButtonsGroup) mainButtonsGroup.SetActive(false);
        if (optionsPanel) optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        if (optionsPanel) optionsPanel.SetActive(false);
        if (mainButtonsGroup) mainButtonsGroup.SetActive(true);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
