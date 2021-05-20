using UnityEngine;

public class Gun : MonoBehaviour
{
    public float damage = -10f;
    public float range = 100f;

    public Camera fpsCam;
    public ParticleSystem muzzleFlash;
    //public Transform muzzleLoc;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        muzzleFlash.Play();
        
        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            Debug.Log(hit.transform.name);

            HealthManager target = hit.transform.GetComponent<HealthManager>();
            if (target != null)
            {
                target.ChangeHealth(damage);
            }
        }

        //if (Physics.Raycast(muzzleLoc.transform.position, muzzleLoc.transform.forward, out hit, range))
        //{
        //    Debug.Log(hit.transform.name);

        //    HealthManager target = hit.transform.GetComponent<HealthManager>();
        //    if (target != null)
        //    {
        //        target.ChangeHealth(damage);
        //    }
        //}
    }
}
