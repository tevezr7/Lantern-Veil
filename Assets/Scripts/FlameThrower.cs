using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class FlameThrower : MonoBehaviour
{
    [SerializeField] private ParticleSystem ShootingSystem;
    [SerializeField] private ParticleSystem OnFireSystemPrefab;
    [SerializeField] private FlameEvent flameRadius;
    [SerializeField] private PlayerMagic playerMagic;
    [SerializeField] private float manaPerSecond = 10f;

    [SerializeField] private AudioSource flameAudioSource;
    [SerializeField] private AudioClip flameLoopClip;
    [SerializeField] private float flameFadeOutTime = 0.2f;
    private Coroutine fadeOutRoutine;

    [SerializeField] private int BurningDPS = 5;
    [SerializeField] private float BurningDuration = 3f; // kept for future use
    private bool isCasting = false;               // tracks if the flame is active
    private Coroutine manaDrainRoutine;          // reference to the mana drain coroutine


    // Simple stack pool
    private Stack<ParticleSystem> onFirePool;
    private readonly Dictionary<EnemyAI, ParticleSystem> EnemyParticleSystems = new Dictionary<EnemyAI, ParticleSystem>();

    private void Awake()
    {
        onFirePool = new Stack<ParticleSystem>();

        if (flameRadius != null)
        {
            flameRadius.OnEnemyEnter += StartDamagingEnemy;
            flameRadius.OnEnemyExit += StopDamagingEnemy;
        }
    }

    private void Start()
    {
        
    }

    public void StartFlame()
    {
        Debug.Log("StartFlame() CALLED");


        if (isCasting) return;   // don't restart if already active

        if (playerMagic != null && playerMagic.magic <= 0f)
        {
            Debug.Log("Not enough magic to start flame.");
            return;
        }

        isCasting = true;

        if (ShootingSystem != null)
        {
            ShootingSystem.gameObject.SetActive(true);
            ShootingSystem.Clear(true);
            ShootingSystem.Play(true);
        }

        if (flameRadius != null)
            flameRadius.gameObject.SetActive(true);

        if (flameRadius != null)
        {
            // Enable FlameEvent logic
            flameRadius.enabled = true;

            // Enable the collider even if it's on a child object
            var col = flameRadius.GetComponentInChildren<Collider>();
            if (col != null)
                col.enabled = true;
        }

        //sound effect can be added here
        StartFlameSound();

        if (playerMagic != null) // start mana drain coroutine
        {
            if (manaDrainRoutine != null)
                StopCoroutine(manaDrainRoutine);

            manaDrainRoutine = StartCoroutine(DrainMana());
        }
    }

    public void StopFlame()
    {
        if (!isCasting) return;  // already off
        isCasting = false;

        if (ShootingSystem != null)
        {
            ShootingSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            ShootingSystem.gameObject.SetActive(false);
        }

        foreach (var enemy in flameRadius.EnemiesInRange)
        {
            StopDamagingEnemy(enemy);
        }
        flameRadius.ForceClear();

        StartCoroutine(DisableFlameColliderNextFrame());

        if (manaDrainRoutine != null)
        {
            StopCoroutine(manaDrainRoutine);
            manaDrainRoutine = null;
        }
        //end audio here
        StopFlameSound();
    }

    private IEnumerator DisableFlameColliderNextFrame()
    {
        // Wait one physics update so EXIT events fire
        yield return new WaitForFixedUpdate();

        flameRadius.enabled = false;

        var col = flameRadius.GetComponentInChildren<Collider>();
        if (col != null)
            col.enabled = false;
    }

    private void StartFlameSound()
    {
        if (fadeOutRoutine != null)
        {
            StopCoroutine(fadeOutRoutine);
            fadeOutRoutine = null;
        }

        if (flameAudioSource != null && flameLoopClip != null)
        {
            if (!flameAudioSource.isPlaying)
            {
                flameAudioSource.clip = flameLoopClip;
                flameAudioSource.loop = true;
                flameAudioSource.volume = 1f;
                flameAudioSource.Play();
            }
        }
    }

    private void StopFlameSound()
    {
        if (flameAudioSource == null || !flameAudioSource.isPlaying)
            return;

        fadeOutRoutine = StartCoroutine(FadeOutFlameSound());
    }

    private IEnumerator FadeOutFlameSound()
    {
        float startVolume = flameAudioSource.volume;
        float t = 0f;

        while (t < flameFadeOutTime)
        {
            t += Time.deltaTime;
            float k = 1f - (t / flameFadeOutTime);
            flameAudioSource.volume = startVolume * k;
            yield return null;
        }

        flameAudioSource.Stop();
        flameAudioSource.volume = startVolume;
    }


    private void OnDestroy()
    {
        // Unsubscribe
        if (flameRadius != null)
        {
            flameRadius.OnEnemyEnter -= StartDamagingEnemy;
            flameRadius.OnEnemyExit -= StopDamagingEnemy;
        }

        // Stop all currently burning enemies and release their particle systems
        if (EnemyParticleSystems.Count > 0)
        {
            // iterate a snapshot to avoid modifying during enumeration
            var snapshot = new List<KeyValuePair<EnemyAI, ParticleSystem>>(EnemyParticleSystems);
            foreach (var kv in snapshot)
            {
                var enemyAI = kv.Key;
                var ps = kv.Value;

                if (enemyAI != null && enemyAI.TryGetComponent<EnemyHealth>(out var eh))
                {
                    eh.StopBurning();
                    eh.OnDeath -= HandleEnemyHealthDeath;
                }

                if (ps) ReleaseOnFireSystem(ps);

                EnemyParticleSystems.Remove(enemyAI);
            }
        }

        // Destroy pooled particle instances (optional but clean)
        if (onFirePool != null)
        {
            while (onFirePool.Count > 0)
            {
                var p = onFirePool.Pop();
                if (p) Destroy(p.gameObject);
            }
        }
    }

    private void StartDamagingEnemy(EnemyAI enemy)
    {
        Debug.Log("StartDamagingEnemy: " + enemy.name);
        if (enemy == null) return;
        if (EnemyParticleSystems.ContainsKey(enemy)) return;

        if (enemy.TryGetComponent<EnemyHealth>(out var eh) && eh.isBurnable)
        {
            // Begin burn and subscribe to death for cleanup
            eh.StartBurning(BurningDPS);
            eh.OnDeath += HandleEnemyHealthDeath;

            var onFireSystem = GetOnFireSystem();
            if (!onFireSystem) return;

            // Attach and play
            onFireSystem.transform.SetParent(enemy.transform, false);
            onFireSystem.transform.localPosition = Vector3.zero;

            var main = onFireSystem.main;
            main.loop = true;
            onFireSystem.Play(true);

            EnemyParticleSystems.Add(enemy, onFireSystem);
        }
    }

    private void StopDamagingEnemy(EnemyAI enemy)
    {
        if (enemy == null) return;

        if (EnemyParticleSystems.TryGetValue(enemy, out var ps))
        {
            // Remove first to avoid double-release from multiple paths
            EnemyParticleSystems.Remove(enemy);

            if (enemy.TryGetComponent<EnemyHealth>(out var eh))
            {
                eh.StopBurning();
                eh.OnDeath -= HandleEnemyHealthDeath;
            }

            if (ps) ReleaseOnFireSystem(ps);
        }
    }

    // EnemyHealth death callback -> route to StopDamagingEnemy
    private void HandleEnemyHealthDeath(EnemyHealth deadHealth)
    {
        if (!deadHealth) return;

        var ai = deadHealth.GetComponent<EnemyAI>();
        if (ai != null) StopDamagingEnemy(ai);

        // ensure unsubscribed
        deadHealth.OnDeath -= HandleEnemyHealthDeath;
    }

    // --- Pool helpers ---

    private ParticleSystem GetOnFireSystem()
    {
        ParticleSystem ps = null;

        if (onFirePool != null && onFirePool.Count > 0)
        {
            ps = onFirePool.Pop();
        }
        else
        {
            if (!OnFireSystemPrefab)
            {
                return null;
            }
            ps = Instantiate(OnFireSystemPrefab);
        }

        if (ps)
        {
            // reset state for reuse
            ps.Clear(true);
            ps.gameObject.SetActive(true);
        }

        return ps;
    }

    private void ReleaseOnFireSystem(ParticleSystem ps)
    {
        if (!ps) return;

        // stop & clear, detach, disable, push back to pool
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.transform.SetParent(null, false);
        ps.gameObject.SetActive(false);

        onFirePool.Push(ps);
    }

    private IEnumerator DrainMana()
    {
        const float tickInterval = 1f; // 1 hit per second

        while (isCasting)
        {
            if (playerMagic == null)
                yield break;

            bool ok = playerMagic.TryUseMagic(manaPerSecond);
            if (!ok)
            {
                Debug.Log("Out of magic, stopping flame.");
                StopFlame();
                yield break;
            }

            yield return new WaitForSeconds(tickInterval);
        }
    }

    // --- External spell toggles ---

}
