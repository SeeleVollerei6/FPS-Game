using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField] public int HP = 100;
    private Animator animator;
    private NavMeshAgent navAgent;

    public bool isDead;

    public Collider zombieHand;

    public float HearingSensitivity = 1.0f; 

    private Coroutine isBlind;
    public GameObject player;

    [Header("Loot setting")]
    [SerializeField] private GameObject[] lootPrefabs;
    [SerializeField] private float lootSpawnHeight = 1.5f;
    [SerializeField] private float lootSpawnChance = 0.7f;
    [SerializeField] private float lootLifeTime = 10f;

    public Transform hipsBone;
    public float maxRootSeparation = 10f;

    public bool canSeePlayer { get; private set; }

    public enum LootType
    {
        Nothing,
        Rifle,
        Pistol,
        PistolAmmoBox,
        RifelAmmoBox,
        AmmoBox,
        Throwable,
        Tactical,
        Firstaid,
        EnergyDot
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
        animator.SetBool("isDead", false);
        player = GameObject.FindWithTag("Player");
        hipsBone = animator.GetBoneTransform(HumanBodyBones.Hips);
    }

    void Update()
    {
        if (!isDead){
        float dist = Vector3.Distance(transform.position, hipsBone.position);
        if (dist > maxRootSeparation)
            {
                Debug.LogWarning("Zombie ragdoll separated too far! Deleting...");
                gameObject.SetActive(false);
                ObjectPoolManager.Instance.ReturnToPool("Zombie", gameObject, 0f);
            }
        }
    }

    public void TakeDamage(int damageAmount)
    {
        HP -= damageAmount;

        if (HP <= 0)
        {   
            int randomValue = UnityEngine.Random.Range(0,2);
            if (randomValue == 0)
            {
                animator.SetTrigger("DIE1");
            }
            if (randomValue == 1)
            {
                animator.SetTrigger("DIE2");
            }
            
            isDead = true;
            SoundManager.Instance.zombieChannel2.PlayOneShot(SoundManager.Instance.zombieDeath);
            if (isDead)
            {
                SpawnRandomLoot();
                SpawnEnergyDot();
                animator.SetBool("isDead", true);
                if (navAgent)
                {
                    navAgent.isStopped = true;
                    navAgent.ResetPath();
                    navAgent.velocity = Vector3.zero;
                }
            }
        }
        else
        {
            animator.SetTrigger("DAMAGE");
            SoundManager.Instance.zombieChannel2.PlayOneShot(SoundManager.Instance.zombieHurt);
        }
    }

    private void OnEnable()
    {
        GameEvents.OnGunshotFired += RespondToSound;
    }

    private void OnDisable()
    {
        GameEvents.OnGunshotFired -= RespondToSound;
    }

    private void RespondToSound(Vector3 soundPos, float range)
    {
        if (isDead) return;

        float dist = Vector3.Distance(transform.position, soundPos);
        if (dist <= range * HearingSensitivity)
        {
            animator.SetBool("Chasing", true);
            animator.SetBool("Partrolling", false);
        }
    }

    public void Blind()
    {
        if (isBlind != null)
        {
            StopCoroutine(isBlind);
        }
        isBlind = StartCoroutine(BlindEnemy());
    }

    private IEnumerator BlindEnemy()
    {
        animator.SetBool("isBlind", true);

        yield return new WaitForSeconds(8f);

        animator.SetBool("isBlind", false);
    }

    private void SpawnRandomLoot()
    {
       if (UnityEngine.Random.Range(0f, 1f) > lootSpawnChance)
        {
            return;
        }

        LootType lootType = GetRandomLootType();
        
        SpawnLoot(lootType);
    }

    private void SpawnEnergyDot()
    {
        SpawnLoot(LootType.EnergyDot);
    }

    private LootType GetRandomLootType()
    {
        float randomValue = UnityEngine.Random.Range(0f, 1f);
        
        if (randomValue < 0.1f)
            return LootType.AmmoBox;
        else if (randomValue < 0.2f)
            return LootType.PistolAmmoBox;
        else if (randomValue < 0.25f)
            return LootType.RifelAmmoBox;
        else if (randomValue < 0.35f)
            return LootType.Pistol;
        else if (randomValue < 0.4f)
            return LootType.Rifle;
        else if (randomValue < 0.45f)
            return LootType.Throwable;
        else if (randomValue < 0.5f)
            return LootType.Tactical;
        else if (randomValue < 0.55f)
            return LootType.Firstaid;
        else
            return LootType.Nothing;
    }

    private void SpawnLoot(LootType lootType)
    {
        if (lootType == LootType.Nothing) return;

        Vector3 spawnPosition = transform.position + Vector3.up * lootSpawnHeight;

        string lootTag = "";

        switch (lootType)
        {
            case LootType.Rifle: lootTag = "AK74"; break;
            case LootType.Pistol: lootTag = "M1911"; break;
            case LootType.PistolAmmoBox: lootTag = "PistolAmmoBox"; break;
            case LootType.RifelAmmoBox: lootTag = "RifelAmmoBox"; break;
            case LootType.AmmoBox: lootTag = "Ammobox"; break;
            case LootType.Throwable: lootTag = "RGD-5"; break;
            case LootType.Tactical: lootTag = "Smoke"; break;
            case LootType.Firstaid: lootTag = "Firstaid"; break;
            case LootType.EnergyDot: lootTag = "EnergyDot"; break;
        }

        if (!string.IsNullOrEmpty(lootTag))
        {
            SpawnLootItemFromPool(lootTag, spawnPosition);
        }
    }

    private void SpawnLootItemFromPool(string tag, Vector3 position)
    {
        GameObject loot = ObjectPoolManager.Instance.SpawnFromPool(tag, position, Quaternion.identity);
        if (loot != null)
        {
            ObjectPoolManager.Instance.ReturnToPool(tag, loot, lootLifeTime);
        }
    }

    public void SetSpeed(float speed)
    {
        navAgent.speed = speed;
    }

    public void SetHealth(int health)
    {
        HP = health;
    }

    public void EnableAttackCollider()
    {
        zombieHand.enabled = true;
    }

    public void DisableAttackCollider()
    {
        zombieHand.enabled = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Blind();
        }
    }

    // private void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.red;
    //     Gizmos.DrawSphere(transform.position, 2.5f);

    //     Gizmos.color = Color.blue;
    //     Gizmos.DrawSphere(transform.position, 18f);

    //     Gizmos.color = Color.green;
    //     Gizmos.DrawSphere(transform.position, 21f);
    // }
}
