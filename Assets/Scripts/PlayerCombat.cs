using UnityEditor.MPE;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    private PlayerHealth playerHealth;
    private EnemyHealth enemyHealth;
    private bool isBlocking;
    private bool isAttacking;
    public AudioSource playerAudio;
    public AudioClip blockClip;
    public AudioClip swingClip;

    [Header("Attack")]
    public float attackWindup = 0.2f; //still haven't implemented
    public float attackDelay = 0.5f; 
    public float attackDamage = 25f;
    private float nextAttack = 0f; // time when next attack is allowed
    public float attackRange = 2f;
    public float attackRadius = 1.5f;
    public float powerMult = 1.2f;
    public bool IsAttacking => isAttacking;
    public LayerMask attackLayerMask; // layers that can be hit by attacks

    [Header("Blocking")]
    public float blockPercent = 75;
    public float blockAngle = 90f; 
    public Transform blockOrigin; // where the block is centered (usually player transform)                            
    public bool IsBlocking => isBlocking;
    private float lastDodgeTime = -999f;

    [Header("Dodge")]
    private float dodgeEndTime;
    public float dodgeFrameDuration = 0.2f;
    public float iFrameCooldown = 0.4f;
    public bool isDodging => Time.time < dodgeEndTime; // true if currently dodging 


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
        float currentHealth = playerHealth.health;
        float damageTaken = playerHealth.LastDamageTaken;
        float initialDamage = attackDamage; 
        if (blockOrigin == null)
        {
            blockOrigin = transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isBlocking && isDodging) isBlocking = false;

    }

    public void Attack(bool state)
    {
        //still need to implement windup and attack animation
        isAttacking = state;
        if (!isAttacking) return;
        if (Time.time < nextAttack) return; // still in cooldown

        nextAttack = Time.time + attackDelay; // set next attack time
        Vector3 origin = transform.position + transform.forward * attackRange + Vector3.up; // Adjust for player height
        Collider[] hitColliders = Physics.OverlapSphere(origin, attackRadius, attackLayerMask); // Detect enemies in attack radius
        playerAudio.PlayOneShot(swingClip);
        foreach (Collider hitCollider in hitColliders) // Loop through all detected colliders
        {
            // Apply damage to the hit enemy
            EnemyHealth enemyHealth = hitCollider.GetComponentInParent<EnemyHealth>(); 
            if (enemyHealth != null) // Check if the collider has an EnemyHealth component
            {
                enemyHealth.TakeDamage(attackDamage, transform.position); // Apply damage
            }
        }
    }

    public void Block(bool state)
    {
        isBlocking = state; //not sure which script to implement in, but enemy recoil after being blocked would be nice
    }

    

    public void PowerAttack()
    {
        //longer windup animation when we get the animations. everything rn is temporary until we have animations

        

        //future code to break blocks or stagger enemies
        
    }

    

    //power attacks that break blocks? 

    public void Parry()
    {
        //perfect timing block that staggers enemies?
    }

    public void JumpAttack()
    {

    }

    //jump attacks that can't be blocked?
    public bool BlockCone(Vector3 attackerPosition)
    {

        Vector3 targetDir = (attackerPosition - blockOrigin.position).normalized; // Direction from player to attacker

        targetDir.y = 0; // Ignore vertical difference

        float angleToAttacker = Vector3.Angle(blockOrigin.forward, targetDir);

        return angleToAttacker <= blockAngle / 2f;
    }   

    public void DodgeLogic()
    {
        // Prevent dodge if still in cooldown
        if (Time.time < lastDodgeTime + iFrameCooldown) return;
        dodgeEndTime = Time.time + dodgeFrameDuration; // Set the end time for dodging
        lastDodgeTime = Time.time;

    }
    public float PreventDamage(float damageTaken, Vector3 attackerPosition)
    {
        if (isDodging)
            return 0f; // No damage taken while dodging
        else
        if (isBlocking && BlockCone(attackerPosition))
        {
            playerAudio.PlayOneShot(blockClip);
            return damageTaken * (1 - blockPercent / 100f); // Reduce damage if blocking and within block cone
        }
        else return damageTaken;
    }
}
