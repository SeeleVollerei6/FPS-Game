using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int bulletDamage;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Target"))
        {
            // print(collision.gameObject.name);
            CreateBulletImpactEffect(collision);
        }

        if (collision.gameObject.CompareTag("Wall"))
        {
            CreateBulletImpactEffect(collision);
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (collision.gameObject.GetComponentInParent<Enemy>().isDead == false)
            {
                collision.gameObject.GetComponentInParent<Enemy>().TakeDamage(bulletDamage);
            }
            CreateBloodEffect(collision);
        }

        if (collision.gameObject.CompareTag("Head"))
        {
            if (collision.gameObject.GetComponentInParent<Enemy>().isDead == false)
            {
                collision.gameObject.GetComponentInParent<Enemy>().TakeDamage(bulletDamage*2);
            }
            CreateBloodEffect(collision);
        }

        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Obstacle obstacle = collision.gameObject.GetComponent<Obstacle>();
            if (obstacle != null)
            {
                obstacle.TakeDamage(bulletDamage);
            }
            Barrel barrel = collision.gameObject.GetComponent<Barrel>();
            if (barrel != null)
            {
                collision.gameObject.GetComponent<Barrel>().TakeDamage(bulletDamage);
            }
            CreateBulletImpactEffect(collision);
        }
        Destroy(gameObject);
    }

    void CreateBulletImpactEffect(Collision collision)
    {
        ContactPoint contact = collision.contacts[0];

        GameObject hole = Instantiate(
            GlobalReferences.Instance.bulletImpactEffect, contact.point, Quaternion.LookRotation(contact.normal)
        );

        hole.transform.SetParent(collision.gameObject.transform);
    }
    
    void CreateBloodEffect(Collision collision)
    {
        ContactPoint contact = collision.contacts[0];

        GameObject blood = Instantiate(
            GlobalReferences.Instance.bloodSprayEffect,contact.point,Quaternion.LookRotation(contact.normal)
        );

        blood.transform.SetParent(collision.gameObject.transform);
    }
}
