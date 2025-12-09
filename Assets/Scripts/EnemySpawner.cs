using UnityEngine;
using System;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] wave1Enemies;
    public GameObject[] wave2Enemies;
    public GameObject[] wave3Enemies;
    public Transform[] spawnPoints;

    public event Action OnAllWavesCompleted;
    public event Action<int> OnWaveCompleted;

    private int enemiesAlive = 0;
    private int currentWave = 0;
    public int EnemiesAlive => enemiesAlive;
    public int CurrentWave => currentWave;
    private void Start()
    {
        CheckForExistingEnemies();
    }

    public void StartWave(int waveIndex)
    {
        currentWave = waveIndex;

        GameObject[] wave = null;

        // Wave 1 = Goblins already in scene
        if (waveIndex == 1)
        {
            return;
        }
        else if (waveIndex == 2)
        {
            wave = wave2Enemies;  // spiders
        }
        else if (waveIndex == 3)
        {
            wave = wave3Enemies;  // ogre
        }
        else
        {
            return;
        }

        enemiesAlive = wave.Length;

        for (int i = 0; i < wave.Length; i++)
        {
            var enemy = Instantiate(wave[i], spawnPoints[i].position, Quaternion.identity);
            var health = enemy.GetComponent<EnemyHealth>();
            health.OnDeath += HandleEnemyDeath;
        }
    }

    public void CheckForExistingEnemies()
    {
        GameObject[] existing = GameObject.FindGameObjectsWithTag("Enemy");

        enemiesAlive = existing.Length;
        currentWave = 1;

        foreach (var go in existing)
        {
            EnemyHealth eh = go.GetComponent<EnemyHealth>();
            if (eh != null)
                eh.OnDeath += HandleEnemyDeath;
        }
    }

    private void HandleEnemyDeath(EnemyHealth h)
    {
        Debug.Log("Goblin Died: " + h.gameObject.name);
        Debug.Log("Enemies Alive BEFORE: " + enemiesAlive);

        enemiesAlive--;

        Debug.Log("Enemies Alive AFTER: " + enemiesAlive);

        if (enemiesAlive <= 0)
        {
            Debug.Log("Wave Complete Fired for wave: " + currentWave);

            OnWaveCompleted?.Invoke(currentWave);

            if (currentWave == 3)
                OnAllWavesCompleted?.Invoke();
        }
    }
}
