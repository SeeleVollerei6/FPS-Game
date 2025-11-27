using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalReferences : MonoBehaviour
{
    public static GlobalReferences Instance { get; set; }
    public GameObject bulletImpactEffect;
    public GameObject grenadeExplosionEffect;
    public GameObject smokeExplosionEffect;
    public GameObject bloodSprayEffect;
    public int waveNumber;


    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
}
