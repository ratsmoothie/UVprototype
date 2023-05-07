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

    private void Awake()
    {
        enemyRigidBody = GetComponent<Rigidbody>();
        enemyRigidBody.freezeRotation = true;
        agent = GetComponent<NavMeshAgent>();       

        enemyTransform = GetComponentInChildren<Transform>();
        enemyStats = GetComponentInChildren<EnemyStats>();
        enemySpawner = GetComponent<EnemySpawner>();

        StartCoroutine(WaitForPlayerSpawn());
    }

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

    private void SearchWalkPath()
    {
        float randomZ = UnityEngine.Random.Range(-walkToDistance, walkToDistance);
        float randomX = UnityEngine.Random.Range(-walkToDistance, walkToDistance);

        walkTo = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        //makes sure that the new point to walk towards is actually on the map
        if (Physics.Raycast(walkTo, -transform.up, 2f, isGround))
        {
            setWalkTo = true;
        }
    }

    private void checkDistanceToPlayer()
    {
        distanceToPlayer = Vector3.Distance(transform.position, player.position);
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
        if (healthPickupPrefab != null)
        {
            Instantiate(healthPickupPrefab, enemyTransform.position, Quaternion.identity);
            Debug.Log("Health Pickup Dropped!");
            hasDroppedHealthPickup = true;
        }
        
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
                isSafe = true;
            }
            if (playerInDetectionRadius && enemyHostile)
            {
                Chasing();
                isSafe = false;
            }
            if (playerInAttackRadius && enemyShooting.targetVisible && enemyHostile)
            {
                Attacking();
                isSafe = false;
            }
            if (health <= 0 && !hasDroppedHealthPickup)
            {
                DropHealthPickup();
                enemySpawner.EnemyKilled();
                DestroyEnemy();
            } 
    }
}
