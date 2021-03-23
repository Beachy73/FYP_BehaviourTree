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
        
    }

    public void ExecuteBT()
    {
        BTRootNode.Execute();
    }
}
