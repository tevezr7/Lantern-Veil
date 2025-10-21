using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public float health;
    public float maxHealth; //will inherit from prefab / child class

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
            PlayDeathSound();
            Destroy(gameObject); //temporary death handling
        }
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
