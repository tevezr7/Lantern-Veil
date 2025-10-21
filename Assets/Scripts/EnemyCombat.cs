using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    // Attack
    [Header("Attack")]
    public float attackDelay = 3f;     // cooldown between presses/AI calls
    public float attackDamage = 15f;
    public float attackRange = 1.8f;     // center of swing in front of enemy
    public float attackRadius = 1.1f;     // swing bubble
    public float powerMult = 1.0f;     // scale if enraged, etc.
    public LayerMask attackLayerMask;     // set to Player layer (or what the enemy can hit)

    private bool isAttacking;
    private float nextAttack;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void AttackEvent() //called by animation event
    {
        Attack(true);
    }

    public void Attack(bool state) //NOT DONE, this is just a port from player attack
    {
        //still need to implement windup and attack animation. include enemy telegraphing, etc.
        isAttacking = state;
        if (!isAttacking) return;
        if (Time.time < nextAttack) return; // still in cooldown

        nextAttack = Time.time + attackDelay; // set next attack time
        Vector3 origin = transform.position + transform.forward * attackRange + Vector3.up; // Adjust for player height
        Collider[] hitColliders = Physics.OverlapSphere(origin, attackRadius, attackLayerMask); // Detect enemies in attack radius
        foreach (Collider hitCollider in hitColliders) // Loop through all detected colliders
        {
            // Apply damage to the hit enemy
            PlayerHealth playerHealth = hitCollider.GetComponentInParent<PlayerHealth>();
            if (playerHealth != null) // Check if the collider has an PlayerHealth component
            {
                playerHealth.TakeDamage(attackDamage, transform.position); // Apply damage
            }
        }
    }

}
