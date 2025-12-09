    using UnityEngine;
    using UnityEngine.AI;

    public class EnemyAI : MonoBehaviour
    {
        // References
        public NavMeshAgent agent;
        public Transform player;
        public LayerMask whatIsGround, whatIsPlayer;
        private Animator anim;
        [Header("Vision / LOS")]
        public Transform eyes; // origin for line of sight checks (assign in inspector or defaults to transform)
        public LayerMask obstructionMask; // layers that block sight (walls, environment)

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
        private bool isIdleAllowed = true;

        [Header("Movement")]
        [SerializeField] private float chaseSpeed = 3.5f;
        [SerializeField] private float closeRangeSpeed = 2f;
        [SerializeField] private float stoppingDistanceMargin = 0.1f; // small margin so agent steps inside attack sphere

        // For debug state change detection
        private bool lastInAttackRange;
        private bool lastInSightRange;

        private void Awake()
        {
            // prefer inspector assignment; fallback to FindWithTag if null
            if (player == null)
            {
                var playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null) player = playerObj.transform;
                else
                {
                    playerObj = GameObject.Find("Player");
                    if (playerObj != null) player = playerObj.transform;
                    else Debug.LogWarning($"{name}: Player transform not assigned and not found by tag/name.");
                }
            }

            agent = GetComponent<NavMeshAgent>();
            anim = GetComponent<Animator>();

            if (eyes == null) eyes = transform;
        }

        private void Update()
        {
            if (player == null) return; // safe-guard

            // Update detection booleans: require overlap and clear line of sight
            bool overlapSight = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
            bool overlapAttack = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

            // Only set playerInSightRange true if there is also line of sight
            playerInSightRange = overlapSight && HasLineOfSight();
            playerInAttackRange = overlapAttack && HasLineOfSight();

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
            if (anim) anim.SetBool("isIdleAllowed", isIdleAllowed); // Pass the idle allowed state to the animator
        }

        private bool HasLineOfSight()
        {
            if (player == null) return false;
            Vector3 origin = (eyes != null) ? eyes.position : transform.position;
            Vector3 direction = player.position - origin;
            float distance = direction.magnitude;
            if (distance <= 0.001f) return true;
            // If nothing hit between origin and player -> clear LOS
            return !Physics.Linecast(origin, player.position, obstructionMask);
        }

        private void Patroling()
        {
            isIdleAllowed = true;
            if (agent != null)
            {
                agent.isStopped = false;
                agent.speed = chaseSpeed;
            }

            if (!walkPointSet) SearchWalkPoint();

            if (walkPointSet && agent != null)
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
            isIdleAllowed = false;
            if (player == null || agent == null) return;

            // Set stopping distance once to try to get into attack range
            agent.stoppingDistance = Mathf.Max(0f, attackRange - stoppingDistanceMargin);
            agent.isStopped = false;
            agent.speed = chaseSpeed;
            agent.SetDestination(player.position);

            float distance = Vector3.Distance(transform.position, player.position);
            if (distance < 5f)
            {
                agent.speed = closeRangeSpeed; // slow as we get close
                WithinRange();
            }
        }

        private void WithinRange()
        {
            // Placeholder (use for prepping attack)
        }

        void AttackPlayer()
        {
            isIdleAllowed = false;
            if (agent != null)
            {
                // stop movement cleanly
                if (!agent.isStopped) agent.isStopped = true;
                agent.ResetPath();
            }

            if (player != null)
            {
                var flatTarget = new Vector3(player.position.x, transform.position.y, player.position.z);
                transform.LookAt(flatTarget);  // face the player
            }

            if (!alreadyAttacked)
            {
                if (anim) anim.SetTrigger("Attack");
                alreadyAttacked = true;
                // Delay then reset attack; ResetAttack will only resume movement if player is not still in attack range
                Invoke(nameof(ResetAttack), timeBetweenAttacks);
            }
        }

        private void ResetAttack()
        {
            alreadyAttacked = false;

            if (agent == null) return;
            if (!agent.enabled) return;
            if (!agent.isOnNavMesh) return;

        // Only resume movement if player is no longer in attack range (prevents stepping back mid-attack)
        bool playerStillClose = false;
            if (player != null)
            {
                float dist = Vector3.Distance(transform.position, player.position);
                playerStillClose = dist <= (attackRange + stoppingDistanceMargin);
            }

            if (!playerStillClose && agent != null)
            {
                agent.isStopped = false;
                // clear stoppingDistance to default if desired; otherwise leave so agent keeps correct approach distance
            }
        }

        // Draw detection spheres in the editor to visually debug ranges.
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, sightRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            if (eyes != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(eyes.position, 0.05f);
            }
        }
    }
