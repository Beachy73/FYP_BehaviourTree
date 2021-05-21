using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EnemyBB))]
[RequireComponent(typeof(HealthManager))]
public class EnemyAI : MonoBehaviour
{
    #region Enemy Variables

    private EnemyBB bb;
    private BTNode BTRootNode;
    public NavMeshAgent agent;

    private Material material;

    [SerializeField]
    private float moveSpeed = 5f;

    public Vector3 dir;
    private Vector3 moveLocation;
    public bool isMoving = false;
    private Quaternion lookRotation;
    [SerializeField] private float rotationDamping = 8f;

    private HealthManager healthManager;
    [SerializeField] private float healthRestoreRate = 1f;

    public Vector3 currentLocation;

    public GameObject projectile;

    public AudioSource source;

    #endregion


    // Start is called before the first frame update
    void Start()
    {
        CreateBehaviourTree();
        
        // Execute behaviour tree every 0.1 seconds
        InvokeRepeating("ExecuteBT", 0.1f, 0.1f);

        healthManager = this.GetComponent<HealthManager>();
        source = this.GetComponentInChildren<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isMoving)
        {
            dir = moveLocation - transform.position;
            transform.position += dir.normalized * moveSpeed * Time.deltaTime;

            lookRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationDamping);
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            SetColour(new Color(0, 139, 139));
        }

        // Health Regeneration
        if (GetCurrentHealth() > bb.healthRegenThreshold)
        {
            healthManager.ChangeHealth(Time.deltaTime * healthRestoreRate);
        }
        else if (GetCurrentHealth() <= 0)
        {
            Destroy(gameObject);
        }

        currentLocation = GetComponentInChildren<CapsuleCollider>().transform.position;

        //Debug.Log("Available covers length = " + bb.availableCovers.Length);
    }

    private void CreateBehaviourTree()
    {
        //////////////////////////////////////////////////////////////////////////
        //////////////////// Creating Enemy Behaviour Tree ///////////////////////
        //////////////////////////////////////////////////////////////////////////

        // Reference to Enemy Blackboard
        bb = GetComponent<EnemyBB>();

        // Reference to Nav Mesh Agent
        agent = GetComponentInChildren<NavMeshAgent>();

        material = GetComponentInChildren<MeshRenderer>().material;

        // Create root selector
        Selector rootChild = new Selector(bb);
        BTRootNode = rootChild;

        #region Chase Sequence

        CompositeNode chaseSequence = new Sequence(bb);
        CheckInRange chaseRange = new CheckInRange(bb, this, bb.chaseRange);
        ChasePlayer chasePlayer = new ChasePlayer(bb, this, agent);

        chaseSequence.AddChild(chaseRange);
        chaseSequence.AddChild(chasePlayer);

        #endregion

        #region Shoot Sequence

        CompositeNode shootSequence = new Sequence(bb);
        CheckInRange shootRange = new CheckInRange(bb, this, bb.shootRange);
        ShootPlayer shootPlayer = new ShootPlayer(bb, this, agent);

        shootSequence.AddChild(shootRange);
        shootSequence.AddChild(shootPlayer);

        #endregion

        #region Get Health Sequence
        CompositeNode getHealthSequence = new Sequence(bb);

        HealthNode getHealthCheckNode = new HealthNode(bb, this, bb.findHealthThreshold);
        GetClosestHealthPack getClosestHealthPackNode = new GetClosestHealthPack(bb, this);
        PickupHealthPack pickupHealthPackNode = new PickupHealthPack(bb, this, agent);

        getHealthSequence.AddChild(getHealthCheckNode);
        getHealthSequence.AddChild(getClosestHealthPackNode);
        getHealthSequence.AddChild(pickupHealthPackNode);
        #endregion

        #region Cover Sequence
        /////////////////////////// Cover Sequence ///////////////////////////////

        HealthNode coverHealthNode = new HealthNode(bb, this, bb.findCoverThreshold);
        CheckInCover checkInCoverNode = new CheckInCover(bb, this);
        IsCoverAvailable isCoverAvailableNode = new IsCoverAvailable(bb, this, bb.availableCovers, bb.playerTransform);
        GoToCover goToCoverNode = new GoToCover(bb, this, agent);

        CompositeNode goToCoverSequence = new Sequence(bb);
        goToCoverSequence.AddChild(isCoverAvailableNode);
        goToCoverSequence.AddChild(goToCoverNode);

        CompositeNode findCoverSelector = new Selector(bb);
        findCoverSelector.AddChild(goToCoverSequence);
        findCoverSelector.AddChild(chaseSequence);

        CompositeNode takeCoverSelector = new Selector(bb);
        takeCoverSelector.AddChild(checkInCoverNode);
        takeCoverSelector.AddChild(findCoverSelector);

        CompositeNode mainCoverSequence = new Sequence(bb);
        mainCoverSequence.AddChild(coverHealthNode);
        mainCoverSequence.AddChild(takeCoverSelector);

        #endregion

        #region Patrol Sequence
        CompositeNode patrolSequence = new Sequence(bb);
        GetNextPatrolLocation getNextPatrolLocationNode = new GetNextPatrolLocation(bb, this, agent);
        GoToPatrolPoint goToPatrolPointNode = new GoToPatrolPoint(bb, this, agent);

        patrolSequence.AddChild(getNextPatrolLocationNode);
        patrolSequence.AddChild(goToPatrolPointNode);
        #endregion

        rootChild.AddChild(mainCoverSequence);
        rootChild.AddChild(shootSequence);
        rootChild.AddChild(getHealthSequence);
        rootChild.AddChild(chaseSequence);
        rootChild.AddChild(patrolSequence);

        //Debug.Log("Created BT");
    }

    public void ExecuteBT()
    {
        BTRootNode.Execute();
        //Debug.Log("Running BT");
    }

    public void MoveTo(Vector3 moveLocation)
    {
        isMoving = true;
        this.moveLocation = moveLocation;
    }

    public void StopMovement()
    {
        isMoving = false;
    }

    public void SetColour(Color colour)
    {
        material.color = colour;
    }

    public float GetCurrentHealth()
    {
        return healthManager.GetCurrentHealth();
    }
}


//public class MoveTo : BTNode
//{
//    private EnemyBB eBB;
//    private EnemyAI enemyRef;

//    public MoveTo(EnemyBB bb, EnemyAI enemy) : base(bb)
//    {
//        eBB = (EnemyBB)bb;
//        enemyRef = enemy;
//    }

//    public override BTStatus Execute()
//    {
//        enemyRef.MoveTo(eBB.moveToLocation);
//        return BTStatus.SUCCESS;
//    }
//}

//public class WaitTillAtLocation : BTNode
//{
//    private EnemyBB eBB;
//    private EnemyAI enemyRef;

//    public WaitTillAtLocation(Blackboard bb, EnemyAI enemy) : base(bb)
//    {
//        eBB = (EnemyBB)bb;
//        enemyRef = enemy;
//    }

//    public override BTStatus Execute()
//    {
//        BTStatus currentStatus = BTStatus.RUNNING;

//        if (Vector3.Distance(enemyRef.transform.position, eBB.moveToLocation) <= 1.0f)
//        {
//            currentStatus = BTStatus.SUCCESS;
//        }

//        return currentStatus;
//    }
//}

//public class StopMovement : BTNode
//{
//    private EnemyAI enemyRef;

//    public StopMovement(Blackboard bb, EnemyAI enemy) : base(bb)
//    {
//        enemyRef = enemy;
//    }

//    public override BTStatus Execute()
//    {
//        enemyRef.StopMovement();
//        return BTStatus.SUCCESS;
//    }
//}

public class CheckInRange : BTNode
{
    private EnemyBB eBB;
    private EnemyAI enemyRef;
    private float range;

    public CheckInRange(Blackboard bb, EnemyAI enemy, float rangeDistance) : base(bb)
    {
        eBB = (EnemyBB)bb;
        enemyRef = enemy;
        range = rangeDistance;
    }

    public override BTStatus Execute()
    {
        float distance = Vector3.Distance(eBB.playerLocation, enemyRef.currentLocation);

        //Debug.Log("Checking if player is in range");
        //Debug.Log("Distance = " + distance);
        //Debug.Log("Player location = " + eBB.playerLocation);
        //Debug.Log("Current enemy location = " + enemyRef.currentLocation);
        //Debug.Log("Range checking = " + range);

        if (distance <= range)//eBB.range)
        {
            //Debug.Log("Player is in range");
            return BTStatus.SUCCESS;
        }
        else
        {
            //Debug.Log("Player is NOT in range");
            return BTStatus.FAILURE;
        }
    }
}

public class CheckInCover : BTNode
{
    private EnemyBB eBB;
    private EnemyAI enemyRef;

    public CheckInCover(Blackboard bb, EnemyAI enemy) : base(bb)
    {
        eBB = (EnemyBB)bb;
        enemyRef = enemy;
    }

    public override BTStatus Execute()
    {
        RaycastHit hit;
        Debug.Log("Checking if in cover!");
        if (Physics.Raycast(enemyRef.currentLocation, eBB.playerLocation - enemyRef.currentLocation, out hit))
        {
            
            if (hit.collider.transform != eBB.playerTransform)
            {
                Debug.Log("In cover!");
                return BTStatus.SUCCESS;
            }
        }
        Debug.Log("Not in cover");
        return BTStatus.FAILURE;
    }
}

public class IsCoverAvailable : BTNode
{
    private EnemyBB eBB;
    private EnemyAI enemyRef;
    private Cover[] availableCovers;
    private Transform target;

    public IsCoverAvailable(Blackboard bb, EnemyAI enemy, Cover[] covers, Transform theTarget) : base(bb)
    {
        eBB = (EnemyBB)bb;
        enemyRef = enemy;
        availableCovers = covers;
        target = theTarget;
    }

    public override BTStatus Execute()
    {
        Transform bestSpot = FindBestCoverSpot();
        eBB.SetBestCoverSpot(bestSpot);
        return bestSpot != null ? BTStatus.SUCCESS : BTStatus.FAILURE;
    }

    private Transform FindBestCoverSpot()
    {
        if (eBB.GetBestCoverSpot() != null)
        {
            if (CheckIfSpotIsValid(eBB.GetBestCoverSpot()))
            {
                return eBB.GetBestCoverSpot();
            }
        }
        
        float minAngle = 90.0f;
        Transform bestSpot = null;

        for (int i = 0; i < availableCovers.Length; i++)
        {
            Transform bestSpotInCover = FindBestSpotInCover(availableCovers[i], ref minAngle);
            if (bestSpotInCover != null)
            {
                bestSpot = bestSpotInCover;
            }
        }
        return bestSpot;
    }

    private Transform FindBestSpotInCover(Cover cover, ref float minAngle)
    {
        Transform[] availableSpots = cover.GetCoverSpots();
        Transform bestSpot = null;

        Debug.Log("Available spots length = " + availableSpots.Length);

        for (int i = 0; i < availableSpots.Length; i++)
        {
            Vector3 direction = target.position - availableSpots[i].position;
            if (CheckIfSpotIsValid(availableSpots[i]))
            {
                float angle = Vector3.Angle(availableSpots[i].forward, direction);
                if (angle < minAngle)
                {
                    minAngle = angle;
                    bestSpot = availableSpots[i];
                }
            }
        }
        return bestSpot;
    }

    private bool CheckIfSpotIsValid(Transform spot)
    {
        RaycastHit hit;
        Vector3 direction = target.position - spot.position;

        if (Physics.Raycast(spot.position, direction, out hit))
        {
            if (hit.collider.transform != target)
            {
                return true;
            }
        }
        return false;
    }
}

public class GoToCover : BTNode
{
    private EnemyBB eBB;
    private EnemyAI enemyRef;
    private NavMeshAgent agent;

    public GoToCover(Blackboard bb, EnemyAI enemy, NavMeshAgent navAgent) : base(bb)
    {
        eBB = (EnemyBB)bb;
        enemyRef = enemy;
        agent = navAgent;
    }

    public override BTStatus Execute()
    {
        Transform coverSpot = eBB.GetBestCoverSpot();

        if (coverSpot == null)
        {
            Debug.Log("Coverspot == null");
            return BTStatus.FAILURE;
        }

        enemyRef.SetColour(Color.blue);
        float distance = Vector3.Distance(coverSpot.position, agent.transform.position);

        if (distance > 0.2f)
        {
            Debug.Log("Moving to cover!");
            Debug.Log("Coverspot Pos = " + coverSpot.position);
            agent.isStopped = false;
            agent.SetDestination(coverSpot.position);
            return BTStatus.RUNNING;
        }
        else
        {
            Debug.Log("Reached cover!");
            agent.isStopped = true;
            return BTStatus.SUCCESS;
        }
    }
}

// Chases player and returns running until within 0.2f distance, then returns success
public class ChasePlayer : BTNode
{
    private EnemyBB eBB;
    private EnemyAI enemyRef;
    private NavMeshAgent agent;

    public ChasePlayer(Blackboard bb, EnemyAI enemy, NavMeshAgent navAgent) : base(bb)
    {
        eBB = (EnemyBB)bb;
        enemyRef = enemy;
        agent = navAgent;
    }

    public override BTStatus Execute()
    {
        enemyRef.SetColour(new Color(255, 69, 0));  // orange red

        float distance = Vector3.Distance(eBB.playerLocation, agent.transform.position);
        if (distance < eBB.shootRange || distance > eBB.chaseRange || enemyRef.GetCurrentHealth() <= eBB.findCoverThreshold)
        {
            return BTStatus.SUCCESS;
        }        
        else if (distance > 0.2f)
        {
            Debug.Log("Chasing player!");
            agent.isStopped = false;
            agent.SetDestination(eBB.playerLocation);
            //enemyRef.SetColour(new Color(46, 139, 87)); // sea green
            return BTStatus.RUNNING;
        }
        else
        {
            agent.isStopped = true;
            return BTStatus.SUCCESS;
        }
    }
}

// COMPLETE THIS
public class ShootPlayer : BTNode
{
    private EnemyBB eBB;
    private EnemyAI enemyRef;
    private NavMeshAgent agent;
    private float timer = 0f;
    private float waitingTime = 0.05f;
    private bool hasFired = false;

    public ShootPlayer(Blackboard bb, EnemyAI enemy, NavMeshAgent navAgent) : base(bb)
    {
        eBB = (EnemyBB)bb;
        enemyRef = enemy;
        agent = navAgent;
    }

    public override BTStatus Execute()
    {
        agent.isStopped = true;
        enemyRef.transform.LookAt(eBB.playerLocation, enemyRef.transform.up);
        enemyRef.SetColour(new Color(255, 69, 0));  // orange red

        if (Vector3.Distance(eBB.playerLocation, enemyRef.currentLocation) > eBB.shootRange || enemyRef.GetCurrentHealth() <= eBB.findCoverThreshold)
        {
            return BTStatus.SUCCESS;
        }


        // GUN CODE HERE
        Debug.Log("Firing");
        timer += 1f * Time.deltaTime;

        if (!hasFired)
        {
            Fire();
            hasFired = true;
        }

        if (timer >= waitingTime)
        {
            Fire();
        }
        
        return BTStatus.RUNNING;
    }

    private void Fire()
    {
        MonoBehaviour.Instantiate(enemyRef.projectile, enemyRef.transform, false);
        timer = 0f;
        enemyRef.source.Play();
        Debug.Log("BANG");
    }
}

public class HealthNode : BTNode
{
    private EnemyBB eBB;
    private EnemyAI enemyRef;
    private float threshold;

    public HealthNode(Blackboard bb, EnemyAI enemy, float healthThreshold) : base(bb)
    {
        eBB = (EnemyBB)bb;
        enemyRef = enemy;
        threshold = healthThreshold;
    }

    public override BTStatus Execute()
    {
        //Debug.Log("Checking if health is lower than: " + threshold);
        //Debug.Log("Enemy health = " + enemyRef.GetCurrentHealth());

        if (enemyRef.GetCurrentHealth() <= threshold)
        {
            //Debug.Log("Health LOWER than " + threshold);
            return BTStatus.SUCCESS;
        }
        else
        {
            //Debug.Log("Health HIGHER than " + threshold);
            return BTStatus.FAILURE;
        }
        //return enemyRef.GetCurrentHealth() <= threshold ? BTStatus.SUCCESS : BTStatus.FAILURE;
    }
}

public class GetNextPatrolLocation : BTNode
{
    private EnemyBB eBB;
    private EnemyAI enemyRef;
    private NavMeshAgent agent;

    public GetNextPatrolLocation(Blackboard bb, EnemyAI enemy, NavMeshAgent navAgent) : base(bb)
    {
        eBB = (EnemyBB)bb;
        enemyRef = enemy;
        agent = navAgent;
    }

    public override BTStatus Execute()
    {
        eBB.RandomisePatrolPoint();
        eBB.nextPatrolLoc = eBB.patrolLocations[eBB.locationNumber].position;
        return BTStatus.SUCCESS;
    }
}

public class GoToPatrolPoint : BTNode
{
    private EnemyBB eBB;
    private EnemyAI enemyRef;
    private NavMeshAgent agent;

    public GoToPatrolPoint(Blackboard bb, EnemyAI enemy, NavMeshAgent navAgent) : base(bb)
    {
        eBB = (EnemyBB)bb;
        enemyRef = enemy;
        agent = navAgent;
    }

    public override BTStatus Execute()
    {
        Vector3 patrolSpot = eBB.nextPatrolLoc;
        agent.SetDestination(eBB.nextPatrolLoc);

        if (patrolSpot == null)
        {
            return BTStatus.FAILURE;
        }

        enemyRef.SetColour(Color.red);
        float distance = Vector3.Distance(patrolSpot, agent.transform.position);

        Debug.Log("Current location = " + enemyRef.transform.position);
        Debug.Log("Patrol point location = " + patrolSpot);
        Debug.Log("Distance = " + distance);

        if (Vector3.Distance(eBB.playerLocation, enemyRef.currentLocation) <= eBB.chaseRange || enemyRef.GetCurrentHealth() <= eBB.findHealthThreshold)
        {
            return BTStatus.SUCCESS;
        }
        
        if (distance > 0.2f)
        {
            Debug.Log("Patrolling");
            agent.isStopped = false;
            agent.SetDestination(patrolSpot);
            return BTStatus.RUNNING;
        }
        else
        {
            Debug.Log("Reached patrol point");
            agent.isStopped = true;
            return BTStatus.SUCCESS;
        }
    }
}

public class GetClosestHealthPack : BTNode
{
    private EnemyBB eBB;
    private EnemyAI enemyRef;

    private List<HealthPack> healthPacks;

    public GetClosestHealthPack(Blackboard bb, EnemyAI enemy) : base(bb)
    {
        eBB = (EnemyBB)bb;
        enemyRef = enemy;
    }

    public override BTStatus Execute()
    {
        Debug.Log("Getting health pack location");
        
        healthPacks = new List<HealthPack>();
        HealthPack closestPack = null;
        float distance = Mathf.Infinity;
        Vector3 pos = enemyRef.transform.position;

        foreach (HealthPack hp in GameObject.FindObjectsOfType<HealthPack>())
        {
            healthPacks.Add(hp);
        }

        foreach (HealthPack hp in healthPacks)
        {
            Vector3 diff = hp.transform.position - pos;
            float curDistance = diff.sqrMagnitude;

            if (curDistance < distance)
            {
                closestPack = hp;
                distance = curDistance;
            }
        }

        eBB.closestHealthPack = closestPack;

        if (eBB.closestHealthPack != null)
        {
            return BTStatus.SUCCESS;
        }
        else
        {
            return BTStatus.FAILURE;
        }
    }
}

public class PickupHealthPack : BTNode
{
    private EnemyBB eBB;
    private EnemyAI enemyRef;
    private NavMeshAgent agent;

    public PickupHealthPack(Blackboard bb, EnemyAI enemy, NavMeshAgent navAgent) : base(bb)
    {
        eBB = (EnemyBB)bb;
        enemyRef = enemy;
        agent = navAgent;
    }

    public override BTStatus Execute()
    {
        if (eBB.closestHealthPack == null)
        {
            return BTStatus.FAILURE;
        }

        Vector3 healthLoc = eBB.closestHealthPack.transform.position;

        if (healthLoc == null)
        {
            return BTStatus.FAILURE;
        }

        agent.SetDestination(healthLoc);

        enemyRef.SetColour(Color.cyan);
        float distance = Vector3.Distance(healthLoc, agent.transform.position);

        Debug.Log("Current location = " + enemyRef.transform.position);
        Debug.Log("Health Pack = " + healthLoc);
        Debug.Log("Distance = " + distance);

        if (Vector3.Distance(eBB.playerLocation, enemyRef.currentLocation) <= eBB.shootRange || enemyRef.GetCurrentHealth() <= eBB.findCoverThreshold)
        {
            return BTStatus.SUCCESS;
        }

        if (distance > 0.2f)
        {
            Debug.Log("Getting health pack!");
            agent.isStopped = false;
            agent.SetDestination(healthLoc);
            return BTStatus.RUNNING;
        }
        else
        {
            Debug.Log("Picked up health pack");
            agent.isStopped = true;
            return BTStatus.SUCCESS;
        }
    }
}