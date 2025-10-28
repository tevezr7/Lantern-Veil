using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;  
using System.Collections;

public class FlameEvent : MonoBehaviour
{
    public delegate void EnemyEnteredEvent(EnemyAI enemy); //
    public delegate void EnemyExitedEvent(EnemyAI enemy); 

    public EnemyEnteredEvent OnEnemyEnter; // Event triggered when an enemy enters the flamethrower area
    public EnemyExitedEvent OnEnemyExit; // Event triggered when an enemy exits the flamethrower area

    private List<EnemyAI> enemiesInRange = new List<EnemyAI>();
    
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<EnemyAI>(out EnemyAI enemy)) // Check if the collider belongs to an enemy
        {
            enemiesInRange.Add(enemy); // Add enemy to the list
            OnEnemyEnter?.Invoke(enemy); // Trigger the event when an enemy enters
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.TryGetComponent<EnemyAI>(out EnemyAI enemy)) // Check if the collider belongs to an enemy
        {
            enemiesInRange.Remove(enemy); // Remove enemy from the list
            OnEnemyExit?.Invoke(enemy); // Trigger the event when an enemy exits
        }
    }

    private void OnDestroy()
    {
        foreach(EnemyAI enemy in enemiesInRange)
        {
            OnEnemyExit?.Invoke(enemy); // Ensure exit event is triggered for all enemies still in range
        }

        enemiesInRange.Clear(); // Clear the list
    }
}


