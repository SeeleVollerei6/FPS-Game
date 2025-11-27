using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class ZombieChasingState : StateMachineBehaviour
{
    Transform player;
    NavMeshAgent agent; 

    public float stopChasingDistance = 21f;
    public float attackingDistance = 2.5f;
    public float chaseSpeed = 6f;
    private float minChaseTimer = 0f;
    public float minChaseDuration = 5f;


    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = animator.GetComponent<NavMeshAgent>();
        var inSight = animator.GetComponent<Enemy>();

        agent.speed = chaseSpeed;
        minChaseTimer = minChaseDuration;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (SoundManager.Instance.zombieChannel.isPlaying == false)
        {
            SoundManager.Instance.zombieChannel.clip = SoundManager.Instance.zombieChase;
            SoundManager.Instance.zombieChannel.PlayDelayed(1f);
        }

        minChaseTimer -= Time.deltaTime;

        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.SetDestination(player.position);
            
            LookAtPlayer();
        }

        minChaseTimer -= Time.deltaTime;

        float distanceFromPlayer = Vector3.Distance(player.position, animator.transform.position);

        if (distanceFromPlayer <= stopChasingDistance && minChaseTimer <= 0)
        {
            float giveUpChance = Mathf.Clamp(distanceFromPlayer / 50f, 0.1f, 0.3f);
            if (Random.Range(0f, 1f) < giveUpChance)
            {
                animator.SetBool("Chasing", false);
            }
        }

        if (distanceFromPlayer > stopChasingDistance && minChaseTimer <= 0)
        {
            animator.SetBool("Chasing", false);
        }

        if (distanceFromPlayer <= attackingDistance)
        {
            animator.SetBool("Attacking", true);
        }
    }

    private void LookAtPlayer()
    {
        Vector3 dir = player.position - agent.transform.position;
        agent.transform.rotation = Quaternion.LookRotation(dir);

        var yRot = agent.transform.eulerAngles.y;
        agent.transform.rotation = Quaternion.Euler(0, yRot, 0);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.SetDestination(agent.transform.position);
        }
        SoundManager.Instance.zombieChannel.Stop();
    }
}
