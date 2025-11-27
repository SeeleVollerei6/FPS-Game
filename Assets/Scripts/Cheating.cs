using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;

public class Cheating : MonoBehaviour
{
    public Player player;
    public Weapon rifel;

    void OnCollisionEnter()
    {
        Destroy(gameObject);

        player.HP = 10000;
        player.playerHP.text = $"Health:{player.HP}";

        player.energyDots = 10000;
        player.playerEnergy.text = $"Energy:{player.energyDots}";

        player.isCheating = true;

        Vector3 spawnPosition = player.transform.position + player.transform.forward * 2f + Vector3.up;

        Instantiate(rifel, spawnPosition, quaternion.identity);

        WeaponManager.Instance.equippedLethalType = Throwable.ThrowableType.Grenade;
        WeaponManager.Instance.equippedTacticalType = Throwable.ThrowableType.Smoke;

        WeaponManager.Instance.lethalCount = 999;
        WeaponManager.Instance.tacticalCount = 999;
    }
}
