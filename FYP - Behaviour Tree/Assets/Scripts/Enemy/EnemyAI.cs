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

    #endregion


    // Start is called before the first frame update
    void Start()
    {
        CreateBehaviourTree();
        
        

        



        // Execute behaviour tree every 0.1 seconds
        InvokeRepeating("ExecuteBT", 0.1f, 0.1f);

        healthManager = this.GetComponent<HealthManager>();
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
        healthManager.ChangeHealth(Time.deltaTime * healthRestoreRate);
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

        #region Cover Sequence
        /////////////////////////// Cover Sequence ///////////////////////////////
        CompositeNode coverSequence = new Sequence(bb);
        //coverSequence.AddChild(HealthNode);
        //coverSequence.AddChild(takeCoverSelector);

        CompositeNode takeCoverSelector = new Selector(bb);
        CheckInCover checkInCover = new CheckInCover(bb, this);

        CompositeNode findCoverSelector = new Selector(bb);

        
        CompositeNode goToCoverSequence = new Sequence(bb);
        IsCoverAvailable isCoverAvailable = new IsCoverAvailable(bb, this);
        GoToCover goToCover = new GoToCover(bb, this, agent);
        goToCoverSequence.AddChild(isCoverAvailable);
        goToCoverSequence.AddChild(goToCover);

        #endregion
    }

    public void ExecuteBT()
    {
        BTRootNode.Execute();
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
}


public class MoveTo : BTNode
{
    private EnemyBB eBB;
    private EnemyAI enemyRef;

    public MoveTo(EnemyBB bb, EnemyAI enemy) : base(bb)
    {
        eBB = (EnemyBB)bb;
        enemyRef = enemy;
    }

    public override BTStatus Execute()
    {
        enemyRef.MoveTo(eBB.moveToLocation);
        return BTStatus.SUCCESS;
    }
}

public class WaitTillAtLocation : BTNode
{
    private EnemyBB eBB;
    private EnemyAI enemyRef;

    public WaitTillAtLocation(Blackboard bb, EnemyAI enemy) : base(bb)
    {
        eBB = (EnemyBB)bb;
        enemyRef = enemy;
    }

    public override BTStatus Execute()
    {
        BTStatus currentStatus = BTStatus.RUNNING;

        if (Vector3.Distance(enemyRef.transform.position, eBB.moveToLocation) <= 1.0f)
        {
            currentStatus = BTStatus.SUCCESS;
        }

        return currentStatus;
    }
}

public class StopMovement : BTNode
{
    private EnemyAI enemyRef;

    public StopMovement(Blackboard bb, EnemyAI enemy) : base(bb)
    {
        enemyRef = enemy;
    }

    public override BTStatus Execute()
    {
        enemyRef.StopMovement();
        return BTStatus.SUCCESS;
    }
}

public class CheckInRange : BTNode
{
    private EnemyBB eBB;
    private EnemyAI enemyRef;

    public CheckInRange(Blackboard bb, EnemyAI enemy) : base(bb)
    {
        eBB = (EnemyBB)bb;
        enemyRef = enemy;
    }

    public override BTStatus Execute()
    {
        float distance = Vector3.Distance(eBB.playerLocation, enemyRef.transform.position);

        if (distance <= eBB.range)
        {
            return BTStatus.SUCCESS;
        }
        else
        {
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
        if (Physics.Raycast(enemyRef.transform.position, eBB.playerLocation - enemyRef.transform.position, out hit))
        {
            if (hit.collider.transform != eBB.playerTransform)
            {
                return BTStatus.SUCCESS;
            }
        }
        return BTStatus.FAILURE;
    }
}

public class IsCoverAvailable : BTNode
{
    private EnemyBB eBB;
    private EnemyAI enemyRef;
    private Cover[] availableCovers;
    private Transform target;

    public IsCoverAvailable(Blackboard bb, EnemyAI enemy) : base(bb)
    {
        eBB = (EnemyBB)bb;
        enemyRef = enemy;
        target = eBB.playerTransform;
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

        for (int i = 0; i < availableCovers.Length; i++)
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
            return BTStatus.FAILURE;
        }

        enemyRef.SetColour(Color.blue);
        float distance = Vector3.Distance(coverSpot.position, agent.transform.position);

        if (distance > 0.2f)
        {
            agent.isStopped = false;
            agent.SetDestination(coverSpot.position);
            return BTStatus.RUNNING;
        }
        else
        {
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
        float distance = Vector3.Distance(eBB.playerLocation, agent.transform.position);
        if (distance > 0.2f)
        {
            agent.isStopped = false;
            agent.SetDestination(eBB.playerLocation);
            enemyRef.SetColour(new Color(46, 139, 87)); // sea green
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

    public ShootPlayer(Blackboard bb, EnemyAI enemy, NavMeshAgent navAgent) : base(bb)
    {
        eBB = (EnemyBB)bb;
        enemyRef = enemy;
        agent = navAgent;
    }

    public override BTStatus Execute()
    {
        agent.isStopped = true;
        // GUN CODE HERE
        enemyRef.SetColour(new Color(255, 69, 0));  // orange red
        return BTStatus.RUNNING;
    }
}
