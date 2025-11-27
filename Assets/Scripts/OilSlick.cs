using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class OilSlick : MonoBehaviour
{
    public float slowDownFactor = 0.5f;
    public float slipForce = 5f;
    public float zombieSpeed = 6f;
    public float destructionTime = 15f;

    public float damageCounter = 5f;
    private float timer = 0f;

    private List<GameObject> affectedObjects = new List<GameObject>();

    void Start()
    {
        StartCoroutine(DestroySelf(destructionTime));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.SetSpeedMultiplier(slowDownFactor);
                affectedObjects.Add(other.gameObject);
            }
        }
        if (other.CompareTag("Enemy"))
        {
            var enemy = other.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                enemy.SetSpeed(zombieSpeed * slowDownFactor);
                affectedObjects.Add(other.gameObject);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            timer += Time.deltaTime;
            if (timer >= damageCounter)
            {
                other.GetComponent<Player>().TakeDamage(10);
                timer = 0f;
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            timer = 0;
            var player = other.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.SetSpeedMultiplier(1.0f);
            }
        }
        if (other.CompareTag("Enemy"))
        {
            var enemy = other.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                enemy.SetSpeed(zombieSpeed);
            }
        }
    }

    private IEnumerator DestroySelf(float time)
    {
        yield return new WaitForSeconds(time);

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        foreach (GameObject obj in affectedObjects)
        {
            if (obj != null)
            {
                if (obj.CompareTag("Player"))
                {
                    timer = 0;
                    var player = obj.GetComponent<PlayerMovement>();
                    if (player != null) 
                    {
                        player.SetSpeedMultiplier(1f);
                    }
                }
                else if (obj.CompareTag("Enemy"))
                {
                    var enemy = obj.GetComponentInParent<Enemy>();
                    if (enemy != null) 
                    {
                        enemy.SetSpeed(zombieSpeed);
                    }
                }
            }
        }
        affectedObjects.Clear();
    }
}
