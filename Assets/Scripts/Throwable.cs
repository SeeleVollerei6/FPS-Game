using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.AI;

public class Throwable : MonoBehaviour
{
    [SerializeField] float delay = 3f;
    [SerializeField] float damageRadis = 20f;
    [SerializeField] float explosionForce = 1000f;

    float count;

    bool hasExploded = false;
    public bool hasThrown = false;

    public enum ThrowableType
    {
        None,
        Grenade,
        Smoke
    }

    public ThrowableType throwableType;

    private void Start()
    {
        count = delay;
    }

    private void Update()
    {
        if (hasThrown)
        {
            count -= Time.deltaTime;
            if (count <= 0f && !hasExploded)
            {
                Exploded();
                hasExploded = true;
            }
        }
    }

    private void Exploded()
    {
        GetThrowableEffects();

        Destroy(gameObject);
    }

    private void GetThrowableEffects()
    {
       switch (throwableType)
        {
            case ThrowableType.Grenade:
                GrenadeEffext();
                break;

            case ThrowableType.Smoke:
                SmokeEffext();
                break;
        }
    }

    private void GrenadeEffext()
    {
        GameObject explosionEffect = GlobalReferences.Instance.grenadeExplosionEffect;
        Instantiate(explosionEffect, transform.position, transform.rotation);

        SoundManager.Instance.throwableChannel.PlayOneShot(SoundManager.Instance.grenadeSound);

        Collider[] colliders = Physics.OverlapSphere(transform.position, damageRadis);

        foreach (Collider objInRange in colliders)
        {
            Rigidbody rb = objInRange.GetComponent<Rigidbody>();
            if (rb != null && !rb.GetComponent<Throwable>())
            {
                rb.AddExplosionForce(explosionForce, transform.position, damageRadis);
            }

            if (objInRange.gameObject.GetComponentInParent<Enemy>() && !objInRange.gameObject.GetComponentInParent<Enemy>().isDead)
            {
                objInRange.gameObject.GetComponentInParent<Enemy>().TakeDamage(50);
            }
            if (objInRange.gameObject.GetComponent<Barrel>() && !objInRange.gameObject.GetComponent<Barrel>().exploded)
            {
                objInRange.gameObject.GetComponent<Barrel>().TakeDamage(100);
            }
        }
    }

    private void SmokeEffext()
    {
        GameObject smokeEffect = GlobalReferences.Instance.smokeExplosionEffect;
        Instantiate(smokeEffect, transform.position + Vector3.up * 1.5f, transform.rotation);

        SoundManager.Instance.throwableChannel.PlayOneShot(SoundManager.Instance.grenadeSound);

        Collider[] colliders = Physics.OverlapSphere(transform.position, damageRadis);

        foreach (Collider objInRange in colliders)
        {
            if (objInRange.gameObject.GetComponentInParent<Enemy>() && !objInRange.gameObject.GetComponentInParent<Enemy>().isDead)
            {
                objInRange.gameObject.GetComponentInParent<Enemy>().Blind();
            }
        }
    }
}
