using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    // References
    
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask whatIsGround, whatIsPlayer;

    // Patrolling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    // Attack
    public float timeBetweenAttacks;
    bool alreadyAttacked;

    // States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    private void Awake()
    {
        
        player = GameObject.Find("Player").transform;

        // - Get the NavMeshAgent component attached to this GameObject. The agent controls movement on the NavMesh.
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        // Each frame we update detection booleans using Physics.CheckSphere.
        // Physics.CheckSphere checks whether any collider on the specified LayerMask overlaps a sphere centered at transform.position.
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        
        // The order matters: patrol is the fallback, chase is mid-distance, attack is highest priority.
        if (!playerInSightRange && !playerInAttackRange) Patroling();
        if ((playerInSightRange && !playerInAttackRange)) ChasePlayer();
        if (playerInAttackRange && playerInSightRange) AttackPlayer();
    }

    private void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();

        // If a walk point is set, tell the NavMeshAgent to move there.
        // Note: agent.SetDestination is non-blocking; agent calculates a path and moves over time.
        if (walkPointSet)
            agent.SetDestination(walkPoint);

        // Compute distance to the current walk point to know when we've reached it.
        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        // Walkpoint reached (within 1 unit) -> mark as unset so a new one is chosen on the next patrol cycle.
        // The threshold (1f) is arbitrary; adjust to your agent size and desired accuracy.
        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        // Choose a random point within a square of side length (2 * walkPointRange) centered on the enemy.
        // This simply offsets the enemy's current position by a random X and Z. Y remains the same.
        // The chosen point is not validated against the NavMesh or ground here.
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);
        Vector3 candidate = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        float sampleMaxDistance = 2.0f; // Max distance to sample from candidate point
        NavMeshHit hit;
        if (NavMesh.SamplePosition(candidate, out hit, sampleMaxDistance, NavMesh.AllAreas)) // Check if the candidate point is on the NavMesh
        {
            // Use the nearest point on the NavMesh and mark walkPoint as set.
            walkPoint = hit.position;
            walkPointSet = true;
        }
        else
        {
            walkPointSet = false; // No valid point found; will try again next patrol cycle.
        }

       
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
        //enemy faces player while chasing, slows down when within certain range, then maintains certain distance
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance < 5f)
        {
            agent.speed = 2f; // Slow down
            WithinRange();
        }
        else
        {
            agent.speed = 3.5f; // Default speed
        }

        
    }

    private void WithinRange()
    {
        // This function can be used to handle logic when the player is within a certain range.
        // Currently not implemented.
        // enemy will slow, face player, and prepare to attack
    }

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position); // Stop moving

        var flatTarget = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(flatTarget);  //face the player


        if (!alreadyAttacked)
        {
            var combat = GetComponent<EnemyCombat>();
            if (combat != null) combat.Attack(true);

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks); // calls ResetAttack after cooldown
        }

            
    }
    private void ResetAttack() 
    { 
        alreadyAttacked = false; 
    }

}
