using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBB : Blackboard
{
    #region Enemy Variables

    public Vector3 moveToLocation;
    private HealthManager healthManager;

    public float range;

    #endregion

    #region Player Variables

    public GameObject player;
    public Transform playerTransform;
    public Vector3 playerLocation;
    private HealthManager playerHM;
    public float playerHealth; 

    #endregion


    // Start is called before the first frame update
    void Start()
    {
        healthManager = this.GetComponent<HealthManager>();
        
        if (player)
        {
            playerTransform = player.transform;
            playerHM = player.GetComponent<HealthManager>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (player)
        {
            playerLocation = player.transform.position;
            playerHealth = playerHM.GetCurrentHealth();
        }
    }
}
