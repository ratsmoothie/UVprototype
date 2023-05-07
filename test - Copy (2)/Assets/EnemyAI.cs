using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

//NavMesh Components provided by Dave / GameDevelopment at https://github.com/Unity-Technologies/NavMeshComponents
// Dave / GameDevelopment's YouTube tutorials were also used as a reference for this code.
public class EnemyAI : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player = null;
    public LayerMask isGround;
    public LayerMask isPlayer;
    Rigidbody enemyRigidBody;
    public enemyShooting enemyShooting;
    public GameObject playerObject;
    public Transform enemyTransform;
    EnemyStats enemyStats;
    EnemySpawner enemySpawner;

    [Header("States")]
    public bool patrolling;
    public bool chasing;
    public bool attacking;
    public bool enemyHostile; //enemy will only attack player when checked in the inspector


    [Header("Patrol Pathing")]
    //patrol variables
    public Vector3 walkTo;
    public bool setWalkTo;
    public float walkToDistance;
    private float searchTimer = 0f;
    public float maxSearchTime = 10f;
    //Idling
    private float idleTimer;
    public float minIdleTime;
    public float maxIdleTime;
    public float timeIdled;
    //3rd times the charm
    public int numPatrolPoints = 3;
    public float patrolPointRadius = 10f;
    private List<Vector3> patrolPoints = new List<Vector3>();
    private int currentPatrolIndex = 0;


    [Header("Enemy Stats")]
    public int health;

    //attack variables
    public float attackSpeed;
    public bool readyToAttack;

    [Header("Range Checks")]
    //states
    public float detectionRadius;
    public float attackRadius;
    public bool playerInDetectionRadius = false;
    public bool playerInAttackRadius = false;
    public float distanceToPlayer;


    [Header("Aiming")]
    private float aimDelay; //delay before agent begins shooting, changes based on how close the player is to the agent
    public float closeRangeAimDelay;
    public float mediumRangeAimDelay;
    public float longRangeAimDelay;
    //ranges to determine which aim delay is used
    private float closeRange;
    private float mediumRange;
    private float longRange;
    public bool inCloseRange = false;
    public bool inMediumRange = false;
    public bool inLongRange = false;

    [Header("Drops")]
    public GameObject healthPickupPrefab; 
    private bool hasDroppedHealthPickup = false;

    [Header("Anim States")]
    public bool isWalking;
    public bool isRunning;
    public bool isMovingFiring;
    public bool isFiring;    
    public bool isSafe;
    public bool mobileDeath;
    public bool staticDeath;
    public float deathDelay;

    [Header("Bounds Check")]
    public float lowBound;

    IEnumerator WaitForPlayerSpawn()
    {
        while (playerObject == null)
        {
            yield return new WaitForSeconds(1.0f); // Wait for 1 second (you can adjust the time as needed)

            // Find the player object
            playerObject = GameObject.Find("PlayerObj");
            if (playerObject != null)
            {
                player = playerObject.transform;
                Debug.Log("Player object found!");
            }
            else
            {
                Debug.Log("Player object not found!");
            }
        }
    } 

    private void setRanges()
    {
        //set ranges
        closeRange = attackRadius / 4;
        mediumRange = attackRadius / 2;
        longRange = attackRadius;
    }

    private void setAimDelay()
    {
        if (distanceToPlayer <= closeRange)
        {
            inCloseRange = true;
            inMediumRange = false;
            inLongRange = false;

            aimDelay = closeRangeAimDelay;
        }
        else if (distanceToPlayer > closeRange && distanceToPlayer <= mediumRange)
        {
            inCloseRange = false;
            inMediumRange = true;
            inLongRange = false;
            aimDelay = mediumRangeAimDelay;
        }
        else if (distanceToPlayer > mediumRange && distanceToPlayer <= attackRadius)
        {
            inCloseRange = false;
            inMediumRange = false;
            inLongRange = true;
            aimDelay = longRangeAimDelay;
        }
        else
        {
            inCloseRange = false;
            inMediumRange = false;
            inLongRange = false;
        }
    }

    /*
    private void Patrolling()
    {
        patrolling = true;
        chasing = false;
        attacking = false;

        if (!setWalkTo)
        {
            
            SearchWalkPath();
            idleTimer = UnityEngine.Random.Range(minIdleTime, maxIdleTime);
        }         

        if (setWalkTo)
        {
            agent.SetDestination(walkTo);
            Vector3 destinationPosition = agent.destination;
            transform.LookAt(destinationPosition);
            isWalking = true;
        }

        //Vector3 distanceToDestination = transform.position - walkTo;

        //destination reached
        if (agent.remainingDistance < 1f)
        {
            isWalking = false;
            
            timeIdled += Time.deltaTime;
            if (timeIdled == idleTimer)
            {
                setWalkTo = false;
            }
            
        }
        else if (searchTimer >= maxSearchTime)
        {
            setWalkTo = false;
            searchTimer = 0f;
            agent.ResetPath();
        }

        //increment search timer
        searchTimer += Time.deltaTime;
    }*/

    private void SetNextPatrolDestination()
    {
        if (patrolPoints == null || patrolPoints.Count == 0)
        {
            return;
        }

        agent.SetDestination(patrolPoints[currentPatrolIndex]);

        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
    }  

    List<Vector3> CreatePatrolPoints()
    {
        for (int i = 0; i < numPatrolPoints; i++)
        {
            Vector3 randomPosition = GetRandomPointOnNavMesh(patrolPointRadius);
            patrolPoints.Add(randomPosition);
            
        }
        if (patrolPoints.Count == 0)
        {
            Debug.Log("No patrol points created");
        }
        else
        {
            Debug.Log("Created " + patrolPoints.Count + " patrol points");
        }
        
        return patrolPoints;
    }

    Vector3 GetRandomPointOnNavMesh(float radius)
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * radius;
        randomDirection += transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, radius, NavMesh.AllAreas);
        return hit.position;
    }

    public List<Vector3> GetPatrolPoints()
    {
        return patrolPoints;
    }

    //visualize agent's patrol points
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        foreach (Vector3 point in patrolPoints)
        {
            Gizmos.DrawSphere(point, 0.5f);
        }
    }

    private void checkDistanceToPlayer()
    {
        distanceToPlayer = Vector3.Distance(transform.position, player.position);
    }

   /*
    private void Patrolling()
    {
        //Debug.Log("Patrolling...");
        if (!agent.pathPending && agent.remainingDistance < 1.0f)
        {
            isWalking = false;
            // Remove the current patrol point from the list if it exists
            if (currentPatrolIndex >= patrolPoints.Count)
            {
                currentPatrolIndex = 0;
            }

            SetNextPatrolDestination();
            isWalking = true;

            Debug.Log("Current Destination is " + agent.destination);

            //remove reached point
            patrolPoints.RemoveAt(currentPatrolIndex);
        }
        //searchTimer += Time.deltaTime;
        if (/*(searchTimer >= maxSearchTime) || *///numPatrolPoints == 0)
        //{
            //searchTimer = 0f;
            //CreatePatrolPoints();
        //}

    //}
     

    private void Patrolling()
    {
        if (!agent.pathPending && agent.remainingDistance < 1.0f) //reached destination
        {
            if (patrolPoints == null || patrolPoints.Count == 0) //no patrol points available
            {
                return;
            }

            // Set the next patrol destination
            SetNextPatrolDestination();
            gameObject.GetComponent<NavMeshAgent>().isStopped = false;
            isWalking = true;

        // Check if the agent has reached the current patrol point
        if (Vector3.Distance(transform.position, patrolPoints[currentPatrolIndex]) < 1.0f)
            {
                gameObject.GetComponent<NavMeshAgent>().isStopped = true;
                isWalking = false;
                // Remove the current patrol point
                if (patrolPoints.Count > 1)
                {
                    patrolPoints.RemoveAt(currentPatrolIndex);
                }
                else
                {
                    Debug.Log("No more patrol points left");
                    CreatePatrolPoints();
                }
            }

            Debug.Log("Current Destination is " + agent.destination);
        }
    }


    private void Chasing()
    {
        patrolling = false;
        chasing = true;

        isRunning = true;

        if (enemyShooting.targetVisible)
        {
            agent.SetDestination(player.position);

            transform.LookAt(player);
        }
        else
        {
            agent.SetDestination(enemyShooting.lastSeenPosition);
            transform.LookAt(enemyShooting.lastSeenPosition);
        }
        
    }

    private void Attacking()
    {
        if (health > 0)
        {
            attacking = true;
            chasing = false;
            if (enemyShooting == null)
            {
                return;
            }
            //keep enemy at attack range
            agent.SetDestination(transform.position);

            if (enemyRigidBody.velocity.magnitude > 0f)
            {
                isMovingFiring = true;
                isFiring = false;
            }
            else
            {
                isMovingFiring = false;
                isFiring = true;
            }

            transform.LookAt(player);

            enemyShooting.Invoke("AttackPlayer", aimDelay);
        }
    }

    private void ResetAttack()
    {
        readyToAttack = true;
    }

    public void DestroyEnemy()
    {
        enemyHostile = false;
        Destroy(this.gameObject);
    }

    public void DropHealthPickup()
    {
        float dropRoll = UnityEngine.Random.value;

        if (healthPickupPrefab != null && dropRoll < 0.33f)
        {
            Instantiate(healthPickupPrefab, enemyTransform.position, Quaternion.identity);
            Debug.Log("Health Pickup Dropped!");
            hasDroppedHealthPickup = true;
        }
        
    }

    private void ResetToNearestPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Count == 0) // no patrol points available
        {
            CreatePatrolPoints();
        }
        else
        {
            // Find the nearest patrol point to the current position of the agent
            int nearestIndex = 0;
            float nearestDistance = Vector3.Distance(transform.position, patrolPoints[nearestIndex]);
            for (int i = 1; i < patrolPoints.Count; i++)
            {
                float distance = Vector3.Distance(transform.position, patrolPoints[i]);
                if (distance < nearestDistance)
                {
                    nearestIndex = i;
                    nearestDistance = distance;
                }
            }

            // Reset the agent's position to the nearest patrol point
            agent.Warp(patrolPoints[nearestIndex]);
        }
    }

    void Start()
    {       
     
    }

    private void Awake()
    {
        enemyRigidBody = GetComponent<Rigidbody>();
        enemyRigidBody.freezeRotation = true;      

        enemyTransform = GetComponentInChildren<Transform>();
        enemyStats = GetComponentInChildren<EnemyStats>();
        enemySpawner = GetComponent<EnemySpawner>();

        //navMesh = NavMesh.CalculateTriangulation().navMesh;
        agent = GetComponent<NavMeshAgent>();
        patrolPoints = CreatePatrolPoints();
        currentPatrolIndex = 0;

        SetNextPatrolDestination();

        StartCoroutine(WaitForPlayerSpawn());
    } 

    // Update is called once per frame
    void Update()
    {  
            checkDistanceToPlayer();
            setAimDelay();
            health = enemyStats.currentHealth;

            //check if player is detected by agent
            playerInDetectionRadius = Physics.CheckSphere(transform.position, detectionRadius, isPlayer);
            playerInAttackRadius = Physics.CheckSphere(transform.position, attackRadius, isPlayer);

            if ((!playerInAttackRadius && !playerInDetectionRadius) || !enemyHostile)
            {
                Patrolling();
                //isSafe = true;
            }
            if (playerInDetectionRadius && enemyHostile)
            {
                Chasing();
                //isSafe = false;
            }
            if (playerInAttackRadius && enemyShooting.targetVisible && enemyHostile)
            {
                Attacking();
                //isSafe = false;
            }
            if (health <= 0 && !hasDroppedHealthPickup)
            {
                DropHealthPickup();
                //enemySpawner.EnemyKilled();
                DestroyEnemy();
            }
    }

    void FixedUpdate()
    {
        if (transform.position.y <= lowBound) // out of bounds
        {
            ResetToNearestPatrolPoint();
            Debug.Log("Agent out of bounds... resetting to nearest patrol point.");
        }
    }
}
