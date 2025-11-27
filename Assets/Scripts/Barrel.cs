using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Barrel : MonoBehaviour
{
    public float mass = 100f; 
    
    public PhysicMaterial barrelPhysicMaterial;
    public Material explodedMaterial;
    public PhysicMaterial explodedBarrelPhysicMaterial;

    private Rigidbody rb;
    public int HP = 100;
    public bool exploded;

    public GameObject oilSlickPrefab;

    public void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = mass;
        exploded = false;
    }

    public void TakeDamage(int damage)
    {
        HP -= damage;

        if ( HP <= 0 && exploded == false)
        {
            exploded = true;
            GameObject explosionEffect = GlobalReferences.Instance.grenadeExplosionEffect;
            Instantiate(explosionEffect, transform.position, transform.rotation);
            SoundManager.Instance.throwableChannel.PlayOneShot(SoundManager.Instance.grenadeSound);

            Collider[] colliders = Physics.OverlapSphere(transform.position, 20f);

            foreach (Collider objInRange in colliders)
            {
                Rigidbody rb = objInRange.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddExplosionForce(10000f, transform.position, 20f);
                }

                if (objInRange.gameObject.GetComponentInParent<Enemy>() && !objInRange.gameObject.GetComponentInParent<Enemy>().isDead)
                {
                    objInRange.gameObject.GetComponentInParent<Enemy>().TakeDamage(100);
                }
                if (objInRange.gameObject.GetComponent<Player>())
                {
                    objInRange.gameObject.GetComponent<Player>().TakeDamage(50);
                }
            }

            Renderer renderer = GetComponent<Renderer>();
            renderer.material = explodedMaterial;

            Collider col = GetComponent<Collider>();
            col.material = explodedBarrelPhysicMaterial;
            mass = 20f;

            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 10f))
            {
                Instantiate(oilSlickPrefab, hit.point + Vector3.up * 0.02f, Quaternion.Euler(90,0,0));
            }
        }
    }
}
