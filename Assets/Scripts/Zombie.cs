using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : MonoBehaviour
{
    public GameObject zombieHand;
    public int zombieDamage;

    // Start is called before the first frame update
    void Start()
    {
        zombieHand = GameObject.FindWithTag("ZombieHand");
        zombieHand.GetComponent<ZombieHand>().damage = zombieDamage;
    }
}
