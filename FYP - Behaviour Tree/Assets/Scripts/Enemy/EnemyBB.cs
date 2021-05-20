using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBB : Blackboard
{
    #region Enemy Variables

    public Vector3 moveToLocation;
    private HealthManager healthManager;

    public float chaseRange;
    public float shootRange;

    private Transform bestCoverSpot;
    [SerializeField] public Cover[] availableCovers;

    public float highHealthThreshold = 75f;
    public float lowHealthThreshold = 25f;

    public Transform[] patrolLocations;
    public int locationNumber;
    public int lastLocationNumber;
    public Vector3 nextPatrolLoc;

    #endregion

    #region Player Variables

    public GameObject player;
    public Transform playerTransform;
    public Vector3 playerLocation;
    private HealthManager playerHM;
    public float playerHealth; 

    #endregion


    // Start is called before the first frame update
    void Awake()
    {
        healthManager = this.GetComponent<HealthManager>();
        
        if (player)
        {
            //playerTransform = player.GetComponentInChildren<PlayerTransform>().transform;
            playerTransform = player.transform;
            playerHM = player.GetComponent<HealthManager>();
        }

        locationNumber = UnityEngine.Random.Range(0, patrolLocations.Length);
    }

    // Update is called once per frame
    void Update()
    {
        if (player)
        {
            //playerLocation = player.transform.position;
            playerLocation = playerTransform.position;
            playerHealth = playerHM.GetCurrentHealth();
        }
    }

    public void SetBestCoverSpot(Transform bestCoverSpot)
    {
        this.bestCoverSpot = bestCoverSpot;
    }

    public Transform GetBestCoverSpot()
    {
        return bestCoverSpot;
    }

    public void RandomisePatrolPoint()
    {
        lastLocationNumber = locationNumber;
        locationNumber = UnityEngine.Random.Range(0, patrolLocations.Length);
        //if (locationNumber == lastLocationNumber)
        //{
        //    RandomisePatrolPoint();
        //}
    }
}
