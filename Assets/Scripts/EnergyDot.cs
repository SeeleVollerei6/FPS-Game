using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyDot : MonoBehaviour
{
    public float lifetime = 10f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.CollectOrb();
                ObjectPoolManager.Instance.ReturnToPool("EnergyDot", gameObject, lifetime);
                gameObject.SetActive(false);
            }
        }
    }
}
