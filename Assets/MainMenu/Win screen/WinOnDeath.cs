using UnityEngine;

public class TestWinOnDeath : MonoBehaviour
{
    private void OnDestroy()
    {
        // When THIS enemy gets destroyed, trigger the win screen
        var win = FindObjectOfType<WinScreenController>(true);
        if (win != null)
        {
            win.ShowWinScreen();
        }

        // Track an enemy kill in stats
        if (GameSessionStats.Instance != null)
        {
            GameSessionStats.Instance.OnEnemyKilled();
        }
    }
}
