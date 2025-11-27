using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; set; }
    public AudioSource shootingChannel;
    public AudioSource reloadingChannel;

    public AudioClip M1911Shot;
    public AudioClip AK74Shot;

    public AudioClip M1911Reload;
    public AudioClip AK74Reload;

    public AudioSource emptySound_AK;

    public AudioSource throwableChannel;
    public AudioClip grenadeSound;

    public AudioSource zombieChannel;
    public AudioSource zombieChannel2;
    public AudioClip zombieWalk;
    public AudioClip zombieChase;
    public AudioClip zombieAttack;
    public AudioClip zombieHurt;
    public AudioClip zombieDeath;

    public AudioSource playerChannel;
    public AudioClip playerHurt;
    public AudioClip playerDie;
    public AudioClip gameoverMusic;

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

    public void PlayShootingSound(Weapon.WeaponModel weapon)
    {
        switch (weapon)
        {
            case Weapon.WeaponModel.M1911:
                shootingChannel.PlayOneShot(M1911Shot);
                break;

            case Weapon.WeaponModel.AK74:
                shootingChannel.PlayOneShot(AK74Shot);
                break;
        }
    }

    public void PlayReloadingSound(Weapon.WeaponModel weapon)
    {
        switch (weapon)
        {
            case Weapon.WeaponModel.M1911:
                reloadingChannel.PlayOneShot(M1911Reload);
                break;

            case Weapon.WeaponModel.AK74:
                reloadingChannel.PlayOneShot(AK74Reload);
                break;
        }
    }

    public void PlayEmptySound(Weapon.WeaponModel weapon)
    {
        switch (weapon)
        {
            case Weapon.WeaponModel.M1911:
                emptySound_AK.Play();
                break;

            case Weapon.WeaponModel.AK74:
                emptySound_AK.Play();
                break;
        }
    }
}
