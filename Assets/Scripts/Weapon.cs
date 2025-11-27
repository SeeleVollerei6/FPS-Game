using System;
using System.Collections;
using System.Collections.Generic;
using OpenCover.Framework.Model;
using TMPro;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public bool isActive;
    public int weaponDamage;

    public bool isShooting, readyToShoot;
    bool allowReset = true;
    public float shootingDelay = 2f;

    public int bulletPerBurst = 3;
    public int burstBulletLeft;

    public float spreadIntensity;
    public float hipspreadIntensity;
    public float ADSspreadIntensity;

    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public float BulletSpeed = 30f;
    public float bulletPrefabLifetime = 3f;

    public GameObject muzzleEffect;

    internal Animator animator;

    public float reloadTime;
    public int magazineSize, bulletsLeft;
    public bool isReloading;

    public Vector3 spawnPosition;
    public Vector3 spawnRotation;

    bool isADS;

    public float gunshotRange = 30f;

    public enum WeaponModel
    {
        M1911,
        AK74
    }
    public WeaponModel thisWeaponModel;

    public float maxAimDistance = 1000f;
    public LayerMask aimMask;

    public enum shootingMode
    {
        Single,
        Burst,
        Auto
    }

    public shootingMode currentShootingMode;

    public void Awake()
    {
        readyToShoot = true;
        burstBulletLeft = bulletPerBurst;
        animator = GetComponent<Animator>();

        spreadIntensity = hipspreadIntensity;
    }

    void Update()
    {
        if (isActive)
        {
            foreach (var t in GetComponentsInChildren<Transform>(true))
            {
                t.gameObject.layer = LayerMask.NameToLayer("Weapon");
            }

            if (Input.GetMouseButtonDown(1))
            {
                animator.SetTrigger("enterADS");
                isADS = true;
                HUDManager.Instance.middleDot.SetActive(false);
                spreadIntensity = ADSspreadIntensity;
            }
            if (Input.GetMouseButtonUp(1))
            {
                animator.SetTrigger("exitADS");
                isADS = false;
                HUDManager.Instance.middleDot.SetActive(true);
                spreadIntensity = hipspreadIntensity;
            }

            GetComponent<Outline>().enabled = false;

            if (bulletsLeft == 0 && isShooting)
            {
                SoundManager.Instance.PlayEmptySound(thisWeaponModel);
                isShooting = false;
            }

            if (currentShootingMode == shootingMode.Auto)
            {
                isShooting = Input.GetKey(KeyCode.Mouse0);
            }
            else if (currentShootingMode == shootingMode.Single || currentShootingMode == shootingMode.Burst)
            {
                isShooting = Input.GetKeyDown(KeyCode.Mouse0);
            }

            if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !isReloading && WeaponManager.Instance.CheckAmmoLeft(thisWeaponModel) > 0)
            {
                Reload();
            }

            if (readyToShoot && !isShooting && !isReloading && bulletsLeft <= 0 && WeaponManager.Instance.CheckAmmoLeft(thisWeaponModel) > 0)
            {
                Reload();
            }

            if (readyToShoot && isShooting && bulletsLeft > 0)
            {
                burstBulletLeft = bulletPerBurst;
                Fire();
            }
        }
        else
        {
            foreach (var t in GetComponentsInChildren<Transform>(true))
            {
                t.gameObject.layer = LayerMask.NameToLayer("Default");
            }
        }
    }

    private void Fire()
    {
        GameEvents.TriggerGunshot(transform.position, gunshotRange);

        if (!GetComponentInParent<Player>().isCheating)
        {
            bulletsLeft--;
        }

        muzzleEffect.GetComponent<ParticleSystem>().Play();

        if (isADS)
        {
            animator.SetTrigger("RECOIL_ADS");
        }
        else
        {
            animator.SetTrigger("RECOIL");
        }

        SoundManager.Instance.PlayShootingSound(thisWeaponModel);
        
        readyToShoot = false;
        Vector3 shootingDirection = CalculateDirectionAndSpread().normalized;

        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);

        Player player1 = GetComponentInParent<Player>();
        Bullet bul = bullet.GetComponent<Bullet>();

        if (player1.hasBuff)
        {
            bul.bulletDamage = weaponDamage + weaponDamage;
        }
        if (player1.isCheating)
        {
            bul.bulletDamage = weaponDamage * 10;
        }
        else
        {
            bul.bulletDamage = weaponDamage;
        }

        var bulletCol = bullet.GetComponent<Collider>();
        if (bulletCol)
        {
            foreach (var col in GetComponentsInChildren<Collider>())
                Physics.IgnoreCollision(bulletCol, col, true);

                var player = GameObject.FindWithTag("Player");
                if (player)
                    foreach (var col in player.GetComponentsInChildren<Collider>())
                        Physics.IgnoreCollision(bulletCol, col, true);
        }

        bullet.transform.forward = shootingDirection;

        bullet.GetComponent<Rigidbody>().AddForce(shootingDirection * BulletSpeed, ForceMode.Impulse);

        StartCoroutine(DestroyBullet(bullet, bulletPrefabLifetime));

        if (allowReset)
        {
            Invoke("ResetShot", shootingDelay);
            allowReset = false;
        }

        if (currentShootingMode == shootingMode.Burst && burstBulletLeft > 1)
        {
            burstBulletLeft--;
            Invoke("Fire", shootingDelay);
        }

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Obstacle obstacle = hit.collider.GetComponent<Obstacle>();
            if (obstacle != null)
            {
                obstacle.HitByBullet(hit.point, ray.direction, BulletSpeed);
            }
        }
    }

    private void Reload()
    {
        SoundManager.Instance.PlayReloadingSound(thisWeaponModel);

        animator.SetTrigger("RELOAD");

        isReloading = true;
        Invoke("ReloadCompleted", reloadTime);
    }

        private void ReloadCompleted()
    {
        if (WeaponManager.Instance.CheckAmmoLeft(thisWeaponModel) > magazineSize)
        {
            bulletsLeft = magazineSize;
            WeaponManager.Instance.DecreaseTotalAmmo(bulletsLeft, thisWeaponModel);
        }
        else
        {
            bulletsLeft = WeaponManager.Instance.CheckAmmoLeft(thisWeaponModel);
            WeaponManager.Instance.DecreaseTotalAmmo(bulletsLeft, thisWeaponModel);
        }

        isReloading = false;
        bulletsLeft = magazineSize;
    }

    public void ResetShot()
    {
        readyToShoot = true;
        allowReset = true;
    }

    public Vector3 CalculateDirectionAndSpread()
    {
        Camera playerCamera = Camera.main;
        var cam = playerCamera ? playerCamera : Camera.main;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 targetPoint;
        if (Physics.Raycast(ray, out RaycastHit hit, maxAimDistance, aimMask, QueryTriggerInteraction.Ignore))
            targetPoint = hit.point;
        else
            targetPoint = ray.origin + ray.direction * maxAimDistance;

        Vector3 dir = (targetPoint - bulletSpawn.position).normalized;

        float r = Mathf.Tan(spreadIntensity * Mathf.Deg2Rad);
        Vector2 j = UnityEngine.Random.insideUnitCircle * r;
        Transform ct = cam.transform;
        Vector3 spread = ct.right * j.x + ct.up * j.y;

        return (dir + spread).normalized;
    }

    
    private IEnumerator DestroyBullet(GameObject bullet, float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(bullet);
    }
}
