using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance { get; set; }

    public List<GameObject> weaponSlots;
    public GameObject activeWeaponSlot;

    [Header("Ammo")]
    public int totalRifleAmmo = 0;
    public int totalPistolAmmo = 0;

    [Header("Throwables")]
    public float maxThrowVelocity = 20f;
    public float forceMultiplier = 0.5f;
    public int curveResolution = 30;
    public GameObject grenadePrefab;
    public GameObject smokePrefab;
    public GameObject throwableSpawn;
    public LineRenderer throwableLine;
    private bool isChargingThrow = false;

    [Header("Lethals")]
    public int maxLethalCount = 3;
    public int lethalCount = 0;
    public Throwable.ThrowableType equippedLethalType;

    [Header("Tacticals")]
    public int maxTacticalCount = 2;
    public int tacticalCount = 0;
    public Throwable.ThrowableType equippedTacticalType;

    private void Start()
    {
        activeWeaponSlot = weaponSlots[0];

        equippedLethalType = Throwable.ThrowableType.None;
        equippedTacticalType = Throwable.ThrowableType.None;
    }

    private void Update()
    {
        foreach (GameObject weaponSlot in weaponSlots)
        {
            if (weaponSlot == activeWeaponSlot)
            {
                weaponSlot.SetActive(true);
            }
            else
            {
                weaponSlot.SetActive(false);
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SwitchActiveSlot(0);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SwitchActiveSlot(1);
            }
        }

        HandleThrowInput(KeyCode.G, ref lethalCount, () => ThrowLethal());
        HandleThrowInput(KeyCode.T, ref tacticalCount, () => ThrowTactical());
    }

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

    public void PickupWeapon(GameObject weapon)
    {
        ObjectPoolManager.Instance.CancelReturn(weapon);
        weapon.GetComponent<LootRotation>().enabled = false;
        AddWeaponIntoActiveSlot(weapon);
    }

    private void AddWeaponIntoActiveSlot(GameObject weapon)
    {
        DropCurrentWeapon(weapon);

        weapon.transform.SetParent(activeWeaponSlot.transform, false);
        Weapon weapon1 = weapon.GetComponent<Weapon>();

        weapon.transform.localPosition = new Vector3(weapon1.spawnPosition.x, weapon1.spawnPosition.y, weapon1.spawnPosition.z);
        weapon.transform.localRotation = Quaternion.Euler(weapon1.spawnRotation.x, weapon1.spawnRotation.y, weapon1.spawnRotation.z);

        weapon1.isActive = true;
        weapon1.animator.enabled = true;
    }

    private void DropCurrentWeapon(GameObject weapon)
    {
        if (activeWeaponSlot.transform.childCount > 0)
        {
            var weaponToDrop = activeWeaponSlot.transform.GetChild(0).gameObject;

            weaponToDrop.GetComponent<Weapon>().isActive = false;
            weaponToDrop.GetComponent<Weapon>().animator.enabled = false;

            weaponToDrop.transform.SetParent(weapon.transform.parent);
            weaponToDrop.transform.localPosition = weapon.transform.localPosition;
            weaponToDrop.transform.localRotation = weapon.transform.localRotation;
            weaponToDrop.GetComponent<LootRotation>().enabled = true;

            string tag  = GetWeaponTag(weaponToDrop.GetComponent<Weapon>());

            ObjectPoolManager.Instance.ReturnToPool(tag, weaponToDrop, 10f);
        }
    }

    public void SwitchActiveSlot(int slotNumber)
    {
        if (activeWeaponSlot.transform.childCount > 0)
        {
            Weapon currentWeapon = activeWeaponSlot.transform.GetChild(0).GetComponent<Weapon>();
            currentWeapon.isActive = false;
        }

        activeWeaponSlot = weaponSlots[slotNumber];

                if (activeWeaponSlot.transform.childCount > 0)
        {
            Weapon newWeapon = activeWeaponSlot.transform.GetChild(0).GetComponent<Weapon>();
            newWeapon.isActive = true;
        }
    }

    internal void PickupAmmoBox(AmmoBox ammo)
    {
        switch (ammo.ammoType)
        {
            case AmmoBox.AmmoType.PistolAmmo:
                totalPistolAmmo += ammo.ammoAmount;
                break;

            case AmmoBox.AmmoType.RifleAmmo:
                totalRifleAmmo += ammo.ammoAmount;
                break;

            case AmmoBox.AmmoType.Everything:
                totalPistolAmmo += ammo.ammoAmount/2;
                totalRifleAmmo += ammo.ammoAmount/2;

                int randomLethal = UnityEngine.Random.Range(0, 3);
                int randomTactical = UnityEngine.Random.Range(0, 2);

                lethalCount += randomLethal;
                tacticalCount += randomTactical;
                break;
        }
    }

    internal void DecreaseTotalAmmo(int bulletsLeft, Weapon.WeaponModel thisWeaponModel)
    {
        switch (thisWeaponModel)
        {
            case Weapon.WeaponModel.AK74:
                totalRifleAmmo -= bulletsLeft;
                break;

            case Weapon.WeaponModel.M1911:
                totalPistolAmmo -= bulletsLeft;
                break;
        }
    }

    public string GetWeaponTag(Weapon weapon)
    {
        Weapon.WeaponModel WeaponModel = weapon.thisWeaponModel;
        switch (WeaponModel)
        {
            case Weapon.WeaponModel.AK74:
                return "AK74";

            case Weapon.WeaponModel.M1911:
                return "M1911";
            default:
                return null;
        }
    }

    public int CheckAmmoLeft(Weapon.WeaponModel thisWeaponModel)
    {
        switch (thisWeaponModel)
        {
            case Weapon.WeaponModel.AK74:
                return totalRifleAmmo;

            case Weapon.WeaponModel.M1911:
                return totalPistolAmmo;

            default:
                return 0;
        }
    }

    public void PickupThrowable(Throwable hoveredThrowable)
    {
        if (!hoveredThrowable.hasThrown)
        {
            switch (hoveredThrowable.throwableType)
            {
                case Throwable.ThrowableType.Grenade:
                    PickupThrowableAsLethal(Throwable.ThrowableType.Grenade);
                    break;

                case Throwable.ThrowableType.Smoke:
                    PickupThrowableAsTactical(Throwable.ThrowableType.Smoke);
                    break;
            }
        }
    }

    private void PickupThrowableAsLethal(Throwable.ThrowableType lethal)
    {
        if (equippedLethalType == lethal || equippedLethalType == Throwable.ThrowableType.None)
        {
            equippedLethalType = lethal;

            if (lethalCount < maxLethalCount)
            {
                lethalCount += 1;
                GameObject throwableObj = InteractionManager.Instance.hoveredThrowable.gameObject;

                ObjectPoolManager.Instance.CancelReturn(throwableObj);
                InteractionManager.Instance.ReturnItemToPool(throwableObj); 
                InteractionManager.Instance.hoveredThrowable = null;

                HUDManager.Instance.updateThrowables();
            }
            else
            {
                HUDManager.Instance.updateThrowablesNotice(lethal);
                return;
            }
        }
    }

    private void ThrowLethal()
    {
        string tag = GetThrowableTag(equippedLethalType);
        if (string.IsNullOrEmpty(tag)) return;

        GameObject throwable = ObjectPoolManager.Instance.SpawnFromPool(tag, throwableSpawn.transform.position, Camera.main.transform.rotation);
        
        if (throwable != null)
        {
            Rigidbody rb = throwable.GetComponent<Rigidbody>();

            rb.velocity = Vector3.zero; 
            rb.angularVelocity = Vector3.zero;
            
            Vector3 throwVelocity = Camera.main.transform.forward * (maxThrowVelocity * forceMultiplier);
            rb.velocity = throwVelocity;

            Throwable throwableScript = throwable.GetComponent<Throwable>();
            throwableScript.hasThrown = true;

            lethalCount -= 1;
            if (lethalCount <= 0)
            {
                equippedLethalType = Throwable.ThrowableType.None;
            }
            
            HUDManager.Instance.updateThrowables();
        }
    }

    private string GetThrowableTag(Throwable.ThrowableType type)
    {
        switch (type)
        {
            case Throwable.ThrowableType.Grenade: return "RGD-5";
            case Throwable.ThrowableType.Smoke: return "Smoke";
        }
        return "";
    }

    private void PickupThrowableAsTactical(Throwable.ThrowableType tactical)
    {
        if (equippedTacticalType == tactical || equippedTacticalType == Throwable.ThrowableType.None)
        {
            equippedTacticalType = tactical;

            if (tacticalCount < maxTacticalCount)
            {
                tacticalCount += 1;
                GameObject throwableObj = InteractionManager.Instance.hoveredThrowable.gameObject;

                ObjectPoolManager.Instance.CancelReturn(throwableObj);
                InteractionManager.Instance.ReturnItemToPool(throwableObj); 
                InteractionManager.Instance.hoveredThrowable = null;
                
                HUDManager.Instance.updateThrowables();
            }
            else
            {
                HUDManager.Instance.updateThrowablesNotice(tactical);
                return;
            }
        }
    }

    private void ThrowTactical()
    {
        string tag = GetThrowableTag(equippedTacticalType);
         if (string.IsNullOrEmpty(tag)) return;

        GameObject throwable = ObjectPoolManager.Instance.SpawnFromPool(tag, throwableSpawn.transform.position, Camera.main.transform.rotation);
        
        if (throwable != null)
        {
            Rigidbody rb = throwable.GetComponent<Rigidbody>();
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            Vector3 throwVelocity = Camera.main.transform.forward * (maxThrowVelocity * forceMultiplier);
            rb.velocity = throwVelocity;

            throwable.GetComponent<Throwable>().hasThrown = true;

            tacticalCount -= 1;
            if (tacticalCount <= 0)
            {
                equippedTacticalType = Throwable.ThrowableType.None;
            }
            
            HUDManager.Instance.updateThrowables();
        }
    }

    private void HandleThrowInput(KeyCode key, ref int count, Action throwAction)
    {
        if (count <= 0) return;

        if (Input.GetKeyDown(key))
        {
            isChargingThrow = true;
            forceMultiplier = 0.2f;
            throwableLine.enabled = true;
        }

        if (Input.GetKey(key) && isChargingThrow)
        {
            forceMultiplier += Time.deltaTime * 1.5f; 
            forceMultiplier = Mathf.Clamp01(forceMultiplier);

            DrawCurve();
        }

        if (Input.GetKeyUp(key) && isChargingThrow)
        {
            throwAction.Invoke();
            isChargingThrow = false;
            forceMultiplier = 0f;
            throwableLine.enabled = false;
        }
    }

    private void DrawCurve()
    {
        if (throwableLine == null) return;

        Vector3 startPos = throwableSpawn.transform.position;
        Vector3 velocity = Camera.main.transform.forward * (maxThrowVelocity * forceMultiplier);

        throwableLine.positionCount = curveResolution;
        for (int i = 0; i < curveResolution; i++)
        {
            float time = i * 0.1f;
            Vector3 pos = Projectile.CalculatePosition(startPos, velocity, time);
            throwableLine.SetPosition(i, pos);
        }
    }
}
