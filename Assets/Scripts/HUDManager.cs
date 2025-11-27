using System;
using System.Collections;
using JetBrains.Annotations;
using TMPro;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; set; }

    [Header("Ammo")]
    public TextMeshProUGUI magazineAmmoUI;
    public TextMeshProUGUI totalAmmoUI;
    public Image ammoTypeUI;

    [Header("Weapon")]
    public Image activeWeaponUI;
    public Image unactivateWeaponUI;

    [Header("Throwables")]
    public Image lethalUI;
    public TextMeshProUGUI lethalAmountUI;

    public Image tacticalUI;
    public TextMeshProUGUI tacticalAmountUI;

    public Sprite emptySlot;
    public Sprite greySlot;

    public GameObject middleDot;

    public TextMeshProUGUI throwableNotice;

    public TextMeshProUGUI tooltip;
    public TextMeshProUGUI buff;

    private Coroutine notice;
    private Coroutine tooltips;

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
        Weapon activeWeapon = WeaponManager.Instance.activeWeaponSlot.GetComponentInChildren<Weapon>();
        Weapon unactiveWeapon = GetUnactivateWeaponSlot().GetComponentInChildren<Weapon>();

        if (activeWeapon)
        {
            magazineAmmoUI.text = $"{activeWeapon.bulletsLeft / activeWeapon.bulletPerBurst}";
            totalAmmoUI.text = $"{WeaponManager.Instance.CheckAmmoLeft(activeWeapon.thisWeaponModel)}";

            Weapon.WeaponModel model = activeWeapon.thisWeaponModel;
            ammoTypeUI.sprite = GetAmmoSprite(model);

            activeWeaponUI.sprite = GetWeaponSprite(model);

            if (unactiveWeapon)
            {
                unactivateWeaponUI.sprite = GetWeaponSprite(unactiveWeapon.thisWeaponModel);
            }
        }
        else
        {
            magazineAmmoUI.text = "";
            totalAmmoUI.text = "";

            ammoTypeUI.sprite = emptySlot;

            activeWeaponUI.sprite = emptySlot;
            unactivateWeaponUI.sprite = emptySlot;
        }

        if (WeaponManager.Instance.lethalCount <= 0)
        {
            lethalUI.sprite = greySlot;
        }
        if (WeaponManager.Instance.tacticalCount <= 0)
        {
            tacticalUI.sprite = greySlot;
        }
    }

    private Sprite GetWeaponSprite(Weapon.WeaponModel model)
    {
        switch (model)
        {
            case Weapon.WeaponModel.M1911:
                return Resources.Load<GameObject>("M1911_Weapon").GetComponent<SpriteRenderer>().sprite;

            case Weapon.WeaponModel.AK74:
                return Resources.Load<GameObject>("AK74_Weapon").GetComponent<SpriteRenderer>().sprite;
            default:
                return null;
        }
    }

    private Sprite GetAmmoSprite(Weapon.WeaponModel model)
    {
        switch (model)
        {
            case Weapon.WeaponModel.M1911:
                return Resources.Load<GameObject>("Pistol_Ammo").GetComponent<SpriteRenderer>().sprite;

            case Weapon.WeaponModel.AK74:
                return Resources.Load<GameObject>("Rifle_Ammo").GetComponent<SpriteRenderer>().sprite;

            default:
                return null;
        }
    }

    private GameObject GetUnactivateWeaponSlot()
    {
        foreach (GameObject weaponSlot in WeaponManager.Instance.weaponSlots)
        {
            if (weaponSlot != WeaponManager.Instance.activeWeaponSlot)
            {
                return weaponSlot;
            }
        }
        return null;
    }

    internal void updateThrowables()
    {
        lethalAmountUI.text = $"{WeaponManager.Instance.lethalCount}";
        tacticalAmountUI.text = $"{WeaponManager.Instance.tacticalCount}";

        switch (WeaponManager.Instance.equippedLethalType)
        {
            case Throwable.ThrowableType.Grenade:
                lethalUI.sprite = Resources.Load<GameObject>("Grenade").GetComponent<SpriteRenderer>().sprite;
                break;
        }

        switch (WeaponManager.Instance.equippedTacticalType)
        {
            case Throwable.ThrowableType.Smoke:
                tacticalUI.sprite = Resources.Load<GameObject>("Smoke").GetComponent<SpriteRenderer>().sprite;
                break;
        }
    }

    public void updateThrowablesNotice(Throwable.ThrowableType type)
    {
        if (notice != null) StopCoroutine(notice);
        notice = StartCoroutine(updateThrowablesNoticeText(type));
    }

    private IEnumerator updateThrowablesNoticeText(Throwable.ThrowableType type)
    {
        throwableNotice.gameObject.SetActive(true);
        switch (type)
        {
            case Throwable.ThrowableType.Grenade:
                throwableNotice.text = "Lethal is full";
                break;
            case Throwable.ThrowableType.Smoke:
                throwableNotice.text = "Tactical is full";
                break;
            default:
                throwableNotice.text = "full";
                break;
        }
        yield return new WaitForSeconds(2f);
        throwableNotice.gameObject.SetActive(false);
    }

    public void updateTooltip(int index)
    {
        if (tooltips != null) StopCoroutine(tooltips);
        tooltips = StartCoroutine(updateTooltipText(index));
    }

    private IEnumerator updateTooltipText(int index)
    {
        tooltip.gameObject.SetActive(true);
        
        switch (index)
        {
            case 0:
                tooltip.text = "Energy Stored!";
                break;
            case 1:
                tooltip.text = "Too Far!";
                break;
            case 2:
                tooltip.text = "Tower is full!";
                break;
            default:
                tooltip.text = "Unknown message";
                break;
        }

        yield return new WaitForSeconds(2f);
        tooltip.gameObject.SetActive(false);
    }
}
