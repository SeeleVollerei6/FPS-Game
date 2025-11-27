using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class ZombiePartrollingState : StateMachineBehaviour
{
    float timer;
    public float partrollingTime = 0f;

    Transform player;
    NavMeshAgent agent;

    public float detectionAreaRadius = 18f;
    public float partrolSpeed = 4f;

    List<Transform> wayPointList = new List<Transform>();

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
            player = GameObject.FindGameObjectWithTag("Player").transform;
            agent = animator.GetComponent<NavMeshAgent>();

            agent.speed = partrolSpeed;
            timer = 0;

            GameObject wayPointCluster = GameObject.FindGameObjectWithTag("Waypoints");
            foreach (Transform t in wayPointCluster.transform)
            {
                wayPointList.Add(t);
            }
            Vector3 nextPosition = wayPointList[Random.Range(0, wayPointList.Count)].position;
            agent.SetDestination(nextPosition);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (SoundManager.Instance.zombieChannel.isPlaying == false)
        {
            SoundManager.Instance.zombieChannel.clip = SoundManager.Instance.zombieWalk;
            SoundManager.Instance.zombieChannel.PlayDelayed(1f);
        }

        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            if (agent.remainingDistance <= agent.stoppingDistance + 1f)
            {
                agent.SetDestination(wayPointList[Random.Range(0, wayPointList.Count)].position);
            }
        }

        timer += Time.deltaTime;
        if (timer > partrollingTime)
        {
            animator.SetBool("Partrolling", false);
        }

        float distanceFromPlayer = Vector3.Distance(player.position, animator.transform.position);
        if (distanceFromPlayer < detectionAreaRadius)
        {
            animator.SetBool("Chasing", true);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        agent.SetDestination(agent.transform.position);
        SoundManager.Instance.zombieChannel.Stop();
    }
}
