using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string gameSceneName = "(MAIN)Lantern-Veil";

    [Header("Panels")]
    [SerializeField] private GameObject mainButtonsGroup;   // Play / Options / Story / Credits / Controls / Quit
    [SerializeField] private GameObject optionsPanel;       // Options menu
    [SerializeField] private GameObject storyPanel;         // Story / Lore panel
    [SerializeField] private GameObject creditsPanel;       // Credits panel
    [SerializeField] private GameObject controlsPanel;      // Controls / How to Play panel

    // --- Buttons ---

    public void PlayGame()
    {
        if (SceneFader.I != null)
            SceneFader.I.FadeToScene(gameSceneName);
        else
            SceneManager.LoadScene(gameSceneName);
    }

    // ---------- OPTIONS ----------

    public void OpenOptions()
    {
        if (mainButtonsGroup) mainButtonsGroup.SetActive(false);
        if (storyPanel) storyPanel.SetActive(false);
        if (creditsPanel) creditsPanel.SetActive(false);
        if (controlsPanel) controlsPanel.SetActive(false);
        if (optionsPanel) optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        if (optionsPanel) optionsPanel.SetActive(false);
        if (mainButtonsGroup) mainButtonsGroup.SetActive(true);
    }

    // ---------- STORY PANEL ----------

    public void OpenStory()
    {
        if (storyPanel) storyPanel.SetActive(true);
        if (creditsPanel) creditsPanel.SetActive(false);
        if (controlsPanel) controlsPanel.SetActive(false);
        if (optionsPanel) optionsPanel.SetActive(false);
        if (mainButtonsGroup) mainButtonsGroup.SetActive(false);
    }

    public void CloseStory()
    {
        if (storyPanel) storyPanel.SetActive(false);
        if (mainButtonsGroup) mainButtonsGroup.SetActive(true);
    }

    // ---------- CREDITS PANEL ----------

    public void OpenCredits()
    {
        if (creditsPanel) creditsPanel.SetActive(true);
        if (storyPanel) storyPanel.SetActive(false);
        if (controlsPanel) controlsPanel.SetActive(false);
        if (optionsPanel) optionsPanel.SetActive(false);
        if (mainButtonsGroup) mainButtonsGroup.SetActive(false);
    }

    public void CloseCredits()
    {
        if (creditsPanel) creditsPanel.SetActive(false);
        if (mainButtonsGroup) mainButtonsGroup.SetActive(true);
    }

    // ---------- CONTROLS PANEL ----------

    public void OpenControls()
    {
        if (controlsPanel) controlsPanel.SetActive(true);
        if (storyPanel) storyPanel.SetActive(false);
        if (creditsPanel) creditsPanel.SetActive(false);
        if (optionsPanel) optionsPanel.SetActive(false);
        if (mainButtonsGroup) mainButtonsGroup.SetActive(false);
    }

    public void CloseControls()
    {
        if (controlsPanel) controlsPanel.SetActive(false);
        if (mainButtonsGroup) mainButtonsGroup.SetActive(true);
    }

    // ---------- QUIT ----------

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
