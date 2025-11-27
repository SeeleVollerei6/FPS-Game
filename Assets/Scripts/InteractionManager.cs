using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance { get; set; }

    public Weapon hoveredWeapon = null;
    public AmmoBox hoveredAmmoBox = null;
    public Throwable hoveredThrowable = null;
    public Firstaid hoveredFirstaid = null;

    public GameObject currentPlayer;

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

    private void Update()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            GameObject objectHit = hit.transform.gameObject;

            if (objectHit.GetComponent<Weapon>() && objectHit.GetComponent<Weapon>().isActive == false)
            {
                if (hoveredWeapon)
                {
                    hoveredWeapon.GetComponent<Outline>().enabled = false;
                }

                hoveredWeapon = objectHit.gameObject.GetComponent<Weapon>();
                hoveredWeapon.GetComponent<Outline>().enabled = true;

                if (Input.GetKeyDown(KeyCode.F))
                {
                    WeaponManager.Instance.PickupWeapon(objectHit.gameObject);
                }
            }
            else
            {
                if (hoveredWeapon)
                {
                    hoveredWeapon.GetComponent<Outline>().enabled = false;
                }
            }

            if (objectHit.GetComponent<Throwable>())
            {
                if (hoveredThrowable)
                {
                    hoveredThrowable.GetComponent<Outline>().enabled = false;
                }

                hoveredThrowable = objectHit.gameObject.GetComponent<Throwable>();
                if (!hoveredThrowable.hasThrown)
                {
                    hoveredThrowable.GetComponent<Outline>().enabled = true;
                }

                if (Input.GetKeyDown(KeyCode.F))
                {
                    WeaponManager.Instance.PickupThrowable(hoveredThrowable);
                }
            }

            if (objectHit.GetComponent<Firstaid>())
            {
                if (hoveredFirstaid)
                {
                    hoveredFirstaid.GetComponent<Outline>().enabled = false;
                }

                hoveredFirstaid = objectHit.gameObject.GetComponent<Firstaid>();
                hoveredFirstaid.GetComponent<Outline>().enabled = true;
                
                if (Input.GetKeyDown(KeyCode.F))
                {
                    hoveredFirstaid.HealPlayer(currentPlayer);
                    ReturnItemToPool(objectHit); 
                }
            }
            else
            {
                if (hoveredFirstaid)
                {
                    hoveredFirstaid.GetComponent<Outline>().enabled = false;
                }
            }

            if (objectHit.GetComponent<AmmoBox>())
            {
                if (hoveredAmmoBox)
                {
                    hoveredAmmoBox.GetComponent<Outline>().enabled = false;
                }

                hoveredAmmoBox = objectHit.gameObject.GetComponent<AmmoBox>();
                hoveredAmmoBox.GetComponent<Outline>().enabled = true;

                if (Input.GetKeyDown(KeyCode.F))
                {
                    WeaponManager.Instance.PickupAmmoBox(hoveredAmmoBox);
                    ReturnItemToPool(objectHit); 
                }
            }
            else
            {
                if (hoveredAmmoBox)
                {
                    hoveredAmmoBox.GetComponent<Outline>().enabled = false;
                }
            }
        }
    }

    public void ReturnItemToPool(GameObject obj)
    {
        obj.SetActive(false);
    }
}
