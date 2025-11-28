using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeDamage : MonoBehaviour
{
    [SerializeField] private int damagePerSecond = 1;
    [SerializeField] private float damageInterval = 2f;

    private Dictionary<GameObject, float> nextDamageTime = new Dictionary<GameObject, float>();

    private void OnTriggerEnter(Collider other)
    {
        Enemy zombie = other.GetComponentInParent<Enemy>();

        if (zombie != null && !zombie.isDead)
        {
            zombie.TakeDamage(damagePerSecond);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        Enemy zombie = other.GetComponentInParent<Enemy>();

        if (zombie != null && !zombie.isDead)
        {
            if (!nextDamageTime.ContainsKey(zombie.gameObject) || Time.time >= nextDamageTime[zombie.gameObject])
            {
                zombie.TakeDamage(damagePerSecond);

                float nextTime = Time.time + damageInterval;
                
                if (nextDamageTime.ContainsKey(zombie.gameObject))
                {
                    nextDamageTime[zombie.gameObject] = nextTime;
                }
                else
                {
                    nextDamageTime.Add(zombie.gameObject, nextTime);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Enemy zombie = other.GetComponentInParent<Enemy>();

        if (zombie != null)
        {
            if (nextDamageTime.ContainsKey(zombie.gameObject))
            {
                nextDamageTime.Remove(zombie.gameObject);
            }
        }
    }
}
