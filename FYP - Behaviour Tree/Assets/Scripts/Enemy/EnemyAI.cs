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

    #endregion


    // Start is called before the first frame update
    void Start()
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

        



        // Execute behaviour tree every 0.1 seconds
        InvokeRepeating("ExecuteBT", 0.1f, 0.1f);
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
            enemyRef.SetColour(new Color(0, 139, 139));
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
        enemyRef.SetColour(new Color(255, 69, 0));
        return BTStatus.RUNNING;
    }
}
