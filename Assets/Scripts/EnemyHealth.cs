using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public float health;
    public float maxHealth; //will inherit from prefab / child class
    public event System.Action<EnemyHealth> OnDeath;

    [Header("Hit Reaction")]
    public bool canBeStaggered;
    public bool isBurning;
    public bool isBurnable;
    private Coroutine BurnCoroutine;

    [Header("Audio")]
    [SerializeField] private AudioClip deathSfx;
    [SerializeField][Range(0f, 1f)] private float deathVolume = 1f;
    [SerializeField] private float deathMaxDistance = 30f;

    [SerializeField] private EnemyUI healthBar;

    private void Awake()
    {
        // make sure we have a health bar reference
        if (healthBar == null)
            healthBar = GetComponentInChildren<EnemyUI>();
    }

    void Start()
    {
        if (healthBar == null)
            healthBar = GetComponentInChildren<EnemyUI>();

        health = maxHealth;

        if (healthBar != null)
            healthBar.UpdateHealthBar(health, maxHealth);
    }

    void Update()
    {
        health = Mathf.Clamp(health, 0, maxHealth);
    }

    public void TakeDamage(float damage, Vector3 attackerPosition)
    {
        health -= damage;

        if (healthBar != null)
            healthBar.UpdateHealthBar(health, maxHealth);

        if (health <= 0f)
        {
            Die();
        }
    }

    public void StartBurning(int damagePerSecond)
    {
        isBurning = true;
        if (BurnCoroutine != null)
        {
            StopCoroutine(BurnCoroutine); // stop any existing burn coroutine if already burning
        }
        BurnCoroutine = StartCoroutine(BurnDamage(damagePerSecond)); // start the burn damage coroutine
    }

    private IEnumerator BurnDamage(int DamagePerSecond)
    {
        float minTime = 1f / DamagePerSecond; // time interval between damage ticks
        WaitForSeconds wait = new WaitForSeconds(minTime);
        int damageTick = Mathf.FloorToInt(minTime) + 1; // damage per tick
        TakeDamage(damageTick, transform.position); // initial damage tick

        while (isBurning)
        {
            yield return wait; // wait for the next tick
            TakeDamage(damageTick, transform.position); // apply damage per tick
        }
    }

    public void StopBurning()
    {
        isBurning = false;
        if (BurnCoroutine != null)
        {
            StopCoroutine(BurnCoroutine);
            BurnCoroutine = null;
        }
    }

    void Die()
    {
        PlayDeathSound();

        OnDeath?.Invoke(this); //death event, checks for subscribers

        // destroy the enemy object (sound will keep playing on a temp AudioSource)
        Destroy(gameObject);
    }

    private void PlayDeathSound()
    {
        if (deathSfx == null)
        {
            // optional debug so you can see in console if you forgot to assign a clip
            // Debug.LogWarning($"[EnemyHealth] No deathSfx assigned on {name}");
            return;
        }

        // create a temporary GameObject just to play the sound,
        // so destroying THIS enemy doesn't kill the audio
        GameObject temp = new GameObject("EnemyDeathSFX");
        temp.transform.position = transform.position;

        AudioSource a = temp.AddComponent<AudioSource>();
        a.clip = deathSfx;
        a.volume = deathVolume;
        a.spatialBlend = 1f;  // 3D sound
        a.rolloffMode = AudioRolloffMode.Linear;
        a.maxDistance = deathMaxDistance;
        a.pitch = Random.Range(0.95f, 1.05f);

        a.Play();

        // destroy the temp object after the clip finishes
        Destroy(temp, deathSfx.length / a.pitch);
    }
}
