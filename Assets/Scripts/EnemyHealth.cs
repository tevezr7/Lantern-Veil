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
    [SerializeField] private AudioSource sfxSource;

    [SerializeField] private EnemyUI healthBar;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    private void Awake()
    {
        if (healthBar != null)
        {
            healthBar = GetComponentInChildren<EnemyUI>();
        }
    }
    void Start()
    {
        if (healthBar == null)
            healthBar = GetComponentInChildren<EnemyUI>();
        health = maxHealth;
        healthBar.UpdateHealthBar(health, maxHealth);
    }

    // Update is called once per frame
    void Update()
    {
        health = Mathf.Clamp(health, 0, maxHealth);
    }
    public void TakeDamage(float damage, Vector3 attackerPosition)
    {   
        
        health -= damage;
        healthBar.UpdateHealthBar(health, maxHealth);
        if (health <= 0)
        {
            Die(); //temporary death handling
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
        }
    }
    void Die()
    {
        PlayDeathSound();
        OnDeath?.Invoke(this); //death event, checks for subscribers
        Destroy(gameObject); //temporary death handling
    }
    private void PlayDeathSound()
    {
        if (deathSfx == null) return;

        if (sfxSource != null)
        {
            // small  pitch variation to make it sound less repetitive
            sfxSource.pitch = Random.Range(0.95f, 1.05f);
            sfxSource.PlayOneShot(deathSfx);
        }
        else
        {
          
            AudioSource.PlayClipAtPoint(deathSfx, transform.position);
        }
    }
}
