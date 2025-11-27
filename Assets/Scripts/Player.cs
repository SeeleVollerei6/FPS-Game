using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public int HP = 100;
    public int HPMax = 100;
    public GameObject bloodyScreen;
    public TextMeshProUGUI playerHP;
    public GameObject gameoverUI;
    public GameObject winUI;
    public TextMeshProUGUI playerEnergy;

    bool notWin;

    public bool isPlayerDead;

    public int energyDots = 0;
    public float energyPerDot = 10f;

    public Tower tower;
    public float interactDistance = 8.0f;
    public bool hasBuff;

    public float healCounter = 1f;
    private float timer = 0f;

    public bool isCheating = false;

    private void Start()
    {
        playerHP.text = $"Health:{HP}";
        notWin = true;
    }

    private void Update()
    {
        if (GlobalReferences.Instance.waveNumber > 10 && notWin)
        {
            PlayerWin();
            notWin = false;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryRechargeTower();
        }

        hasBuff = tower.IsInsideLight(transform.position);
        HUDManager.Instance.buff.gameObject.SetActive(hasBuff);
        timer += Time.deltaTime;
        if (timer >= healCounter)
        {
            GetComponent<Player>().HealPerSecond();
            timer = 0f;
        }

        if (tower.currentHP <= 0 && !isPlayerDead)
        {
            TakeDamage(10000);
        }
    }

private void TryRechargeTower()
    {
        if (energyDots <= 0) return;

        float dist = Vector3.Distance(transform.position, tower.transform.position);
        
        if (dist <= interactDistance)
        {
            float missingEnergy = tower.maxHP - tower.currentHP;

            if (missingEnergy <= 0)
            {
                HUDManager.Instance.updateTooltip(2);
                 return; 
            }

            int dotsNeeded = Mathf.CeilToInt(missingEnergy / energyPerDot);

            int dotsToConsume = Mathf.Min(dotsNeeded, energyDots);

            float energyRestored = dotsToConsume * energyPerDot;

            tower.AddEnergy(energyRestored);
            energyDots -= dotsToConsume;

            playerEnergy.text = $"Energy:{energyDots}";
            
            HUDManager.Instance.updateTooltip(0);
        }
        else
        {
            HUDManager.Instance.updateTooltip(1);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        HP -= damageAmount;

        if (HP <= 0)
        {
            PlayerDead();
            isPlayerDead = true;
        }
        else
        {
            StartCoroutine(BloodyScreenEffect());
            playerHP.text = $"Health:{HP}";
            SoundManager.Instance.playerChannel.PlayOneShot(SoundManager.Instance.playerHurt);
        }
    }


    private void PlayerDead()
    {
        GetComponentInChildren<MouseMovement>().enabled = false;
        GetComponent<PlayerMovement>().enabled = false;
        GetComponentInChildren<Animator>().enabled = true;

        playerHP.gameObject.SetActive(false);
        GetComponent<ScreenFader>().StartFade();
        StartCoroutine(ShowGameoverUI());

        SoundManager.Instance.playerChannel.PlayOneShot(SoundManager.Instance.playerDie);

        SoundManager.Instance.playerChannel.clip = SoundManager.Instance.gameoverMusic;
        SoundManager.Instance.playerChannel.PlayDelayed(2f);
    }

    private void PlayerWin()
    {
        GetComponentInChildren<MouseMovement>().enabled = false;
        GetComponent<PlayerMovement>().enabled = false;

        playerHP.gameObject.SetActive(false);
        GetComponent<ScreenFader>().StartFade();
        StartCoroutine(ShowWinUI());
    }

    private IEnumerator ShowGameoverUI()
    {
        yield return new WaitForSeconds(1f);
        gameoverUI.gameObject.SetActive(true);

        int waveSurvived = GlobalReferences.Instance.waveNumber;
        if (waveSurvived -1 > SaveLoadManager.Instance.LoadHighScore())
        {
            SaveLoadManager.Instance.SaveHighScore(waveSurvived - 1);
        }

        StartCoroutine(ReturnMainMenu());
    }

    private IEnumerator ShowWinUI()
    {
        yield return new WaitForSeconds(1f);
        winUI.gameObject.SetActive(true);

        int waveSurvived = GlobalReferences.Instance.waveNumber;
        if (waveSurvived -1 > SaveLoadManager.Instance.LoadHighScore())
        {
            SaveLoadManager.Instance.SaveHighScore(waveSurvived - 1);
        }
        StartCoroutine(ReturnMainMenu());
    }

    private IEnumerator ReturnMainMenu()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator BloodyScreenEffect()
    {
        if (bloodyScreen.activeInHierarchy == false)
        {
            bloodyScreen.SetActive(true);
        }

        var image = bloodyScreen.GetComponentInChildren<Image>();
 
        Color startColor = image.color;
        startColor.a = 1f;
        image.color = startColor;
 
        float duration = 1f;
        float elapsedTime = 0f;
 
        while (elapsedTime < duration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
 
            Color newColor = image.color;
            newColor.a = alpha;
            image.color = newColor;
 
            elapsedTime += Time.deltaTime;
 
            yield return null; ;
        }

        if (bloodyScreen.activeInHierarchy == false)
        {
            bloodyScreen.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ZombieHand") && !other.GetComponentInParent<Enemy>().isDead)
        {
            if (isPlayerDead == false)
            {
                TakeDamage(other.gameObject.GetComponent<ZombieHand>().damage);
            }
        }
    }

    public void CollectOrb()
    {
        energyDots++;
        playerEnergy.text = $"Energy:{energyDots}";
    }

    private void HealPerSecond()
    {
        if (hasBuff && HP < HPMax)
        {
            HP = Mathf.Min(HP + 1, HPMax);
            playerHP.text = $"Health:{HP}";
        }
    }
}
