using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyBB))]
[RequireComponent(typeof(HealthManager))]
public class EnemyAI : MonoBehaviour
{
    #region Enemy Variables

    private EnemyBB bb;
    private BTNode BTRootNode;

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
