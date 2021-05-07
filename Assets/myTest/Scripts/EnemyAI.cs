using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class EnemyAI : MonoBehaviourPun
{

    Transform target;
    NavMeshAgent agent;
    Animator anim;
    bool isDead = false;
    [SerializeField]
    float chaseDistance = 1f;
    [SerializeField]
    float turnSpeed = 5f;


    void Start()
    {
        if(!photonView.IsMine)
        {
            return;
        }
        target = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

    }

    void FixedUpdate()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        var distance = Vector3.Distance(transform.position, target.position);

        if (distance > chaseDistance && !isDead)
        {
            ChasePlayer();
        }
        else
        {
            AttackPlayer();
        }

    }

    public void EnemyDeathAnim()
    {
        isDead = true;
        anim.SetTrigger("isDead");
    }

    void ChasePlayer()
    {
        agent.updateRotation = true;
        agent.updatePosition = true;
        agent.SetDestination(target.position);
        anim.SetBool("isWalking", true);
        anim.SetBool("isAttacking", false);
    }


    void AttackPlayer()
    {
        agent.updateRotation = false;
        Vector3 direction = target.position - transform.position;
        direction.y = 0;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), turnSpeed * Time.deltaTime);
        agent.updatePosition = false;
        anim.SetBool("isWalking", false);
        anim.SetBool("isAttacking", true);
    }

}