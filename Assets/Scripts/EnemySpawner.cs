using UnityEngine;
using System;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] wave1Enemies;
    public GameObject[] wave2Enemies;
    public Transform[] spawnPoints;

    public event Action OnAllWavesCompleted;
    public event Action<int> OnWaveCompleted;

    private int enemiesAlive = 0;
    private int currentWave = 0;

    public void StartWave(int waveIndex)
    {
        currentWave = waveIndex;

        GameObject[] wave = waveIndex == 1 ? wave1Enemies : wave2Enemies;

        enemiesAlive = wave.Length;

        for (int i = 0; i < wave.Length; i++)
        {
            var e = Instantiate(wave[i], spawnPoints[i].position, Quaternion.identity);
            var health = e.GetComponent<EnemyHealth>();

            health.OnDeath += HandleEnemyDeath;
        }
    }

    private void HandleEnemyDeath(EnemyHealth h)
    {
        enemiesAlive--;

        if (enemiesAlive <= 0)
        {
            OnWaveCompleted?.Invoke(currentWave);

            if (currentWave == 2)
                OnAllWavesCompleted?.Invoke();
        }
    }
}
