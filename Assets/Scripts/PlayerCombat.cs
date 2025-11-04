using UnityEditor.MPE;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    private PlayerHealth playerHealth;
    private EnemyHealth enemyHealth;
    private bool isBlocking;
    private bool isAttacking;
    private bool isPowerAttacking;
    public AudioSource playerAudio;
    public AudioClip blockClip;
    public AudioClip swingClip;

    [Header("Attack")]
    public float attackDamage = 25f;
    public float attackRange = 2f;
    public float attackRadius = 1.5f;
    public float powerMult = 1.2f;
    private float currentDamageMult = 1f; // 1 = normal, powerMult when powered

    public bool IsAttacking => isAttacking;
    public bool IsPowerAttacking => isPowerAttacking;
    public LayerMask attackLayerMask; // layers that can be hit by attacks

    [Header("Blocking")]
    public float blockPercent = 75;
    public float blockAngle = 90f; 
    public Transform blockOrigin; // where the block is centered (usually player transform)                            
    public bool IsBlocking => isBlocking;
    private bool canPerfectBlock = false;
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

    public void PAttackStartEvent() 
    { 
        isPowerAttacking = true; 
        currentDamageMult = powerMult;
    }
    public void PAttackEndEvent()
    {
        isPowerAttacking = false;
        currentDamageMult = 1f;
    }
    public void AttackStartEvent() => isAttacking = true;
    public void AttackEndEvent() => isAttacking = false;

    public void AttackEvent() //called by animation event
    {
        Attack(true);
    }

    public void Attack(bool state)
    {
        isAttacking = state;
        if (!isAttacking && !isPowerAttacking) return;
        
        Vector3 origin = transform.position + transform.forward * attackRange + Vector3.up; // Adjust for player height
        Collider[] hitColliders = Physics.OverlapSphere(origin, attackRadius, attackLayerMask); // Detect enemies in attack radius
        playerAudio.PlayOneShot(swingClip);
        foreach (Collider hitCollider in hitColliders) // Loop through all detected colliders
        {
            // Apply damage to the hit enemy
            EnemyHealth enemyHealth = hitCollider.GetComponentInParent<EnemyHealth>(); 
            if (enemyHealth != null) // Check if the collider has an EnemyHealth component
            {
                enemyHealth.TakeDamage(attackDamage * currentDamageMult, transform.position); // Apply damage
            }
        }
    }

    public void BlockEvent() //called by animation event
    {
        
    }

    public void Block(bool state)
    {
        isBlocking = state; //not sure which script to implement in, but enemy recoil after being blocked would be nice
    }



    public void PowerAttack()
    { 
        //if we add block breaking attacks later, we can add a parameter here to specify that blockbreaking is true.
        isPowerAttacking = true;
        currentDamageMult = powerMult;
    }



    //power attacks that break blocks? 



    public void PerfectStart()   // called by animation event
    {
        canPerfectBlock = true;
        Debug.Log("Perfect block window opened!");
    }

    public void PerfectEnd()     // called by animation event
    {
        canPerfectBlock = false;
        Debug.Log("Perfect block window closed.");
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
            if (canPerfectBlock)
            {
                Debug.Log("Perfect Block!");
                playerAudio.PlayOneShot(blockClip); //later a different clip for perfect block
                return 0f; // No damage taken on perfect block
            }
            playerAudio.PlayOneShot(blockClip);
            return damageTaken * (1 - blockPercent / 100f); // Reduce damage if blocking and within block cone
        }
        else return damageTaken;
    }
}
