using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

public class FlameThrower : MonoBehaviour //referencing Llam academy flame thrower tutorial
{
    [SerializeField]
    private ParticleSystem ShootingSystem;
    [SerializeField]
    private ParticleSystem OnFireSystemPrefab;
    [SerializeField]
    private FlameEvent flameRadius;

    [SerializeField]
    private int BurningDPS = 5;
    [SerializeField]
    private float BurningDuration = 3f;

    private ObjectPool<ParticleSystem> onFirePool; // Pool for on-fire particle systems

    private Dictionary<EnemyAI, ParticleSystem> EnemyParticleSystems = new();

    private void Start() //temporary to see if flame thrower works
    {
        if (ShootingSystem)
        {
            ShootingSystem.gameObject.SetActive(true);
            ShootingSystem.Play(true);
            Debug.Log(" FlameThrower test: Playing fire!");
        }
    }
    private void Awake()
    {
        onFirePool = new ObjectPool<ParticleSystem>(CreateOnFireSystem); // Initialize the pool with a method to create new particle systems
        flameRadius.OnEnemyEnter += StartDamagingEnemy;
        flameRadius.OnEnemyExit += StopDamagingEnemy;
    }

    private void StartDamagingEnemy(EnemyAI enemy)
    {
        if (enemy.TryGetComponent<EnemyHealth>(out EnemyHealth isBurnable))
        {
            isBurnable.StartBurning(BurningDPS); // Start the burning effect on the enemy
            ParticleSystem onFireSystem = onFirePool.Get(); // Get a particle system from the pool
            onFireSystem.transform.SetParent(enemy.transform, false); // Parent to the enemy
            onFireSystem.transform.localPosition = Vector3.zero; // Position at the enemy's origin
            ParticleSystem.MainModule main = onFireSystem.main; // Access the main module to modify settings
            main.loop = true; // Ensure the particle system loops while the enemy is burning
            EnemyParticleSystems.Add(enemy, onFireSystem); // Track the particle system for this enemy
        }
    }

    private ParticleSystem CreateOnFireSystem()
    {
        return Instantiate(OnFireSystemPrefab); // Create a new particle system instance    
    }

    private void StopDamagingEnemy(EnemyAI enemy)
    {
        if (EnemyParticleSystems.TryGetValue(enemy, out ParticleSystem ps)) 
        {
            ps.Stop();
            onFirePool.Release(ps);
            EnemyParticleSystems.Remove(enemy);
        }
    }

    public void SpellStartEvent()
    {
        ShootingSystem.gameObject.SetActive(true);
        flameRadius.gameObject.SetActive(true);
    }

    public void SpellEndEvent()
    {
        ShootingSystem.gameObject.SetActive(false);
        flameRadius.gameObject.SetActive(false);
    }

    private void HandleEnemyDeath(EnemyHealth enemy)
    {
        enemy.OnDeath -= HandleEnemyDeath; // Unsubscribe from the death event
    }

};
