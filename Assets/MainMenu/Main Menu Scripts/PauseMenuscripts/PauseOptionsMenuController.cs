using UnityEngine;

public class PauseOptionsMenuController : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject optionsPanel;

    public void OpenOptions()
    {
        pausePanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void BackToPauseMenu()
    {
        optionsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }
}
