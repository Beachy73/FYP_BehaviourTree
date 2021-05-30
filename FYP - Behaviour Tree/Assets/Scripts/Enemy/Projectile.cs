using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed;

    //private Transform player;
    private Vector3 target;
    private Vector3 defaultLoc = new Vector3(0, 1, 0);
    private Vector3 spawnLoc;

    private EnemyBB enemyBB;
    
    // Start is called before the first frame update
    void Start()
    {
        enemyBB = transform.parent.gameObject.GetComponent<EnemyBB>();
        target = enemyBB.playerLocation;
        target.y = transform.position.y;

        spawnLoc = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, spawnLoc) > 1f)
        {
            this.transform.parent = null;
        }

        if (Vector3.Distance(transform.position, target) < 0.2f || Vector3.Distance(transform.position, spawnLoc) > 35f)
        {
            DestroyProjectile();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        HealthManager hm = other.GetComponent<HealthManager>();

        if (hm != null)
        {
            hm.ChangeHealth(-20);
            DestroyProjectile();
        }
    }

    private void DestroyProjectile()
    {
        Destroy(gameObject);
    }
}
