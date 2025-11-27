using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public class ZombieSpawnController : MonoBehaviour
{
    public int initialZombiePerWave = 3;
    public int currentZombiePerWave;

    public float spawnDelay = 1f;
    public float currentDelay;

    public int currentWave = 1;
    public float waveCooldown = 10f;

    public bool inCooldown;
    public float cooldownCounter = 0;
    public float bodyRemainTime = 10f;

    public List<Enemy> currentZombieAlive;

    public TextMeshProUGUI waveOverUI;
    public TextMeshProUGUI cooldownCounterUI;
    public TextMeshProUGUI currentWaveUI;

    [SerializeField] private float enemyHealthMultiplier = 0.1f;
    [SerializeField] private float enemySpeedMultiplier = 0.1f;

    public Transform[] spawnPoints;

    public GameObject player;

    private Coroutine waveCoroutine;

    private void Start()
    {
        currentZombiePerWave = initialZombiePerWave;
        GlobalReferences.Instance.waveNumber = currentWave;

        StartNextWave();
    }

    private void StartNextWave()
    {
        currentZombieAlive.Clear();
        Sun.Instance.OnWaveChanged(currentWave);

        GlobalReferences.Instance.waveNumber = currentWave;
        currentWaveUI.text = "Wave: " + currentWave.ToString();
        StartCoroutine(SpawnWave());
    }

    private IEnumerator SpawnWave()
    {
        foreach (Transform t in spawnPoints)
        {
            for (int i = 0; i < currentZombiePerWave; i++)
            {
                Vector3 spawnOffset = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, UnityEngine.Random.Range(-1f, 1f));
                Vector3 spawnPosition = t.position + spawnOffset;

                var zombie = ObjectPoolManager.Instance.SpawnFromPool("Zombie", spawnPosition, Quaternion.identity);

                Enemy enemyScript = zombie.GetComponent<Enemy>();

                currentZombieAlive.Add(enemyScript);

                yield return new WaitForSeconds(currentDelay);

                ConfigureEnemyData(enemyScript, currentWave);
            }
        }
        waveCoroutine = null;
    }

    private void ConfigureEnemyData(Enemy enemy, int waveNumber)
    {
    int baseHealth = enemy.GetComponent<Enemy>().HP;
    float baseSpeed = enemy.GetComponent<NavMeshAgent>().speed;

    int calculatedHealth = baseHealth + baseHealth * Mathf.RoundToInt(1 + waveNumber * enemyHealthMultiplier);
    float calculatedSpeed = baseHealth + baseSpeed * (1 + waveNumber * enemySpeedMultiplier);
    
    enemy.SetHealth(calculatedHealth);
    enemy.SetSpeed(calculatedSpeed);
    }
    
    private void Update()
    {
        List<Enemy> zombieToRemove = new List<Enemy>();
        foreach (Enemy zombie in currentZombieAlive)
        {
            if (zombie == null || !zombie.gameObject.activeInHierarchy)
            {
                zombieToRemove.Add(zombie);
                continue;
            }
            if (zombie.isDead)
            {
                zombieToRemove.Add(zombie);
                ObjectPoolManager.Instance.ReturnToPool("Zombie", zombie.gameObject, bodyRemainTime);
            }
        }

        foreach (Enemy zombie in zombieToRemove)
        {
            currentZombieAlive.Remove(zombie);
        }

        if (PlayerInTrouble())
        {
            currentDelay = spawnDelay * 2f;
        }
        else
        {
            currentDelay = spawnDelay;
        }

        if (currentZombieAlive.Count == 0 && inCooldown == false && waveCoroutine == null)
        {
            waveCoroutine = StartCoroutine(WaveCooldown());
            inCooldown = true; 
        }

        zombieToRemove.Clear();

        if (inCooldown)
        {
            cooldownCounter -= Time.deltaTime;
        }
        else
        {
            cooldownCounter = waveCooldown;
        }

        cooldownCounterUI.text = cooldownCounter.ToString("F0");

        if (cooldownCounter <= -0.1f && waveCoroutine != null)
        {
            inCooldown = false; 
            cooldownCounter = waveCooldown;
            StopCoroutine(waveCoroutine);
            StartCoroutine(SpawnWave());
        }
    }

    private IEnumerator WaveCooldown()
    {
        currentWave++;
        inCooldown = true;
        waveOverUI.gameObject.SetActive(true);

        yield return new WaitForSeconds(waveCooldown);

        inCooldown = false;
        waveOverUI.gameObject.SetActive(false);

        currentZombiePerWave += 2;

        if (currentWave <= 10)
        {
            StartNextWave();
        }
        else
        {
            currentZombieAlive.Clear();
            Sun.Instance.OnGameWin();
            currentWaveUI.text = "Wave Over!";
            GlobalReferences.Instance.waveNumber += 1;
        }
    }

    private bool PlayerInTrouble()
    {
        int hp = player.GetComponent<Player>().HP;
        float lightTowerHP = player.GetComponent<Player>().tower.currentHP;
        
        if (hp <= 50) return true;
        if (lightTowerHP <= 30f) return true;

        return false;
    }
}
