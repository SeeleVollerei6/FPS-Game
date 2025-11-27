using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Obstacle : MonoBehaviour
{
    public float velocityThreshold = 1f;
    public float timeToStationary = 0.5f;
    public float HP = 150;

    private Rigidbody rb;
    private NavMeshObstacle obstacle;
    private float stationaryTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        obstacle = GetComponent<NavMeshObstacle>();

        obstacle.carving = true;
        obstacle.carveOnlyStationary = false;
    }

    void Update()
    {
        if (rb.velocity.magnitude > velocityThreshold)
        {
            if (obstacle.carving)
            {
                obstacle.carving = false; 
            }
            stationaryTimer = 0f;
            obstacle.enabled = false; 
        }
        else
        {
            stationaryTimer += Time.deltaTime;

            if (stationaryTimer >= timeToStationary)
            {
                if (!obstacle.carving)
                {
                    obstacle.carving = true;
                    obstacle.enabled = true;
                }
            }
        }
    }

    public void HitByBullet(Vector3 bulletPosition, Vector3 bulletDirection, float impactForce)
    {
        Vector3 baseForce = bulletDirection.normalized * impactForce;

        rb.AddForceAtPosition(baseForce, bulletPosition, ForceMode.Impulse);

        float heightFactor = bulletPosition.y - transform.position.y;
        if (heightFactor < 0.5f)
        {
            rb.AddForce(Vector3.up * (impactForce * 0.2f), ForceMode.Impulse);
        }
    }

    public void TakeDamage(int damage)
    {
        HP -= damage;
        
        if ( HP <= 0 )
        {
            Destroy(gameObject);
        }
    }
}
