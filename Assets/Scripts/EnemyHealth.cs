using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public float health;
    public float maxHealth; //will inherit from prefab / child class

    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        health = Mathf.Clamp(health, 0, maxHealth);
    }
    public void TakeDamage(float damage, Vector3 attackerPosition)
    {   
        
        health -= damage;
        if (health <= 0) Destroy(gameObject); //temporary death handling
    }
}
