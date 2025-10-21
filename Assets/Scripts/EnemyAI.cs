using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    // References

    public NavMeshAgent agent;
    public Transform player;
    public LayerMask whatIsGround, whatIsPlayer;
    private Animator anim;

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

    // For debug state change detection
    private bool lastInAttackRange;
    private bool lastInSightRange;

    private void Awake()
    {
        var playerObj = GameObject.Find("Player");
        if (playerObj != null) player = playerObj.transform;
        else Debug.LogWarning($"{name}: GameObject.Find(\"Player\") returned null. Check player name in scene or assign Transform in inspector.");

        // - Get the NavMeshAgent component attached to this GameObject. The agent controls movement on the NavMesh.
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (player == null) return; // safe-guard

        // Each frame we update detection booleans using Physics.CheckSphere.
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        // Quick debug log when state changes (prevents spamming)
        if (playerInAttackRange != lastInAttackRange || playerInSightRange != lastInSightRange)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            Debug.Log($"{name}: dist={dist:F2}, sight={playerInSightRange}, attack={playerInAttackRange}, agent.stoppingDistance={agent?.stoppingDistance}");
            lastInAttackRange = playerInAttackRange;
            lastInSightRange = playerInSightRange;
        }

        // State machine
        if (!playerInSightRange && !playerInAttackRange) Patroling();
        else if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        else if (playerInAttackRange && playerInSightRange) AttackPlayer();

        bool isWalking = agent != null && agent.velocity.magnitude > 0.01f && !playerInAttackRange;
        if (anim) anim.SetBool("isWalking", isWalking);
    }

    private void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;
        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);
        Vector3 candidate = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        float sampleMaxDistance = 2.0f; // Max distance to sample from candidate point
        NavMeshHit hit;
        if (NavMesh.SamplePosition(candidate, out hit, sampleMaxDistance, NavMesh.AllAreas))
        {
            walkPoint = hit.position;
            walkPointSet = true;
        }
        else
        {
            walkPointSet = false;
        }
    }

    private void ChasePlayer()
    {
        if (player == null || agent == null) return;

        // Ensure agent attempts to get inside attackRange.
        // Keep a small margin so the agent will step inside the attack sphere.
        float margin = 0.1f;
        agent.stoppingDistance = Mathf.Max(0f, attackRange - margin);

        agent.SetDestination(player.position);

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
        // Placeholder
    }

    void AttackPlayer()
    {
        if(!agent.isStopped) agent.isStopped = true; // Stop the agent from moving
        if (agent != null) agent.SetDestination(transform.position); // Stop moving

        var flatTarget = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(flatTarget);  //face the player

        if (!alreadyAttacked)
        {
            if (anim) anim.SetTrigger("Attack");
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks); // calls ResetAttack after cooldown
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    // Draw detection spheres in the editor to visually debug ranges.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
