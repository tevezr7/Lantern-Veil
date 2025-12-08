using UnityEngine;

public class GameSessionStats : MonoBehaviour
{
    public static GameSessionStats Instance { get; private set; }

    [Header("Runtime Stats")]
    public int healthPotionsUsed;
    public int magicPotionsUsed;
    public int enemiesKilled;

    // Combined
    public int TotalPotionsUsed => healthPotionsUsed + magicPotionsUsed;

    public float timeSurvived;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void OnHealthPotionUsed()
    {
        healthPotionsUsed++;
    }

    public void OnMagicPotionUsed()
    {
        magicPotionsUsed++;
    }

    public void OnEnemyKilled()
    {
        enemiesKilled++;
    }
}
