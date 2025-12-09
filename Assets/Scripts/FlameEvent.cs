using System;
using System.Collections.Generic;
using UnityEngine;

public class FlameEvent : MonoBehaviour
{
    // Use events to encapsulate subscription (other scripts will += / -=)
    public event Action<EnemyAI> OnEnemyEnter;
    public event Action<EnemyAI> OnEnemyExit;

    private readonly List<EnemyAI> enemiesInRange = new List<EnemyAI>();

    // Expose read-only view for other systems that want to query current enemies
    public IEnumerable<EnemyAI> EnemiesInRange => enemiesInRange;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("FlameEvent ENTER: " + other.name);
        var ai = other.GetComponentInParent<EnemyAI>();
        if (ai) Debug.Log("EnemyAI found: " + ai.name);
        if (other.TryGetComponent<EnemyAI>(out EnemyAI enemy))
        {
            // avoid duplicate entries when an enemy has multiple colliders
            if (enemiesInRange.Contains(enemy)) return;

            enemiesInRange.Add(enemy);
            OnEnemyEnter?.Invoke(enemy);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<EnemyAI>(out EnemyAI enemy))
        {
            if (!enemiesInRange.Contains(enemy)) return;

            enemiesInRange.Remove(enemy);
            OnEnemyExit?.Invoke(enemy);
        }
    }

    public void ForceClear()
    {
        foreach (var enemy in enemiesInRange)
            OnEnemyExit?.Invoke(enemy);

        enemiesInRange.Clear();
    }
    private void OnDestroy()
    {
        // Make a copy to be safe if listeners modify the list during callbacks
        var copy = enemiesInRange.ToArray();
        foreach (var enemy in copy)
        {
            OnEnemyExit?.Invoke(enemy);
        }

        enemiesInRange.Clear();
    }
}