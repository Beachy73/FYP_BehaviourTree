using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPack : MonoBehaviour
{
    [Range(0, 100)]
    public int healAmount = 20;
    //private AudioSource soundEffect;
    public AudioClip healSound;

    private void Start()
    {
        //soundEffect = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        HealthManager healthManager = other.GetComponent<HealthManager>();

        if ((healthManager != null) && (healthManager.GetCurrentHealth() != healthManager.maxHealth))
        {
            AudioSource.PlayClipAtPoint(healSound, transform.position);
            healthManager.ChangeHealth(healAmount);
            Destroy(gameObject);
        }
    }
}
