using UnityEngine;

public class PauseOptionsMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject controlsPanel;   // NEW

    // ---- OPTIONS ----
    public void OpenOptions()
    {
        pausePanel.SetActive(false);
        controlsPanel.SetActive(false);   // make sure controls is hidden
        optionsPanel.SetActive(true);
    }

    public void BackToPauseMenu()
    {
        optionsPanel.SetActive(false);
        controlsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    // ---- CONTROLS ----
    public void OpenControls()
    {
        // hide everything except controls
        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);
        controlsPanel.SetActive(true);
    }

    public void CloseControls()
    {
        // go back to options menu
        controlsPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }
}
