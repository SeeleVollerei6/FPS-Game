using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Firstaid : MonoBehaviour
{
    public int healAmount = 50;

    public void HealPlayer(GameObject interactor)
    {
        int playerHealth = interactor.GetComponent<Player>().HP;
        if (playerHealth > 0)
        {
            playerHealth += healAmount;

            if (playerHealth > interactor.GetComponent<Player>().HPMax && !interactor.GetComponent<Player>().isCheating)
            {
                playerHealth = interactor.GetComponent<Player>().HPMax;
            }

            interactor.GetComponent<Player>().playerHP.text = $"Health:{playerHealth}";
        }
        
        // Destroy(gameObject);
    }
}
