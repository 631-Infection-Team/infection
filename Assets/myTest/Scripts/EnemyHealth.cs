 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class EnemyHealth : MonoBehaviourPun
{

    public float enemyHealth = 3;
    EnemyAI enemyAI;

    private void Start()
    {
        enemyAI = GetComponent<EnemyAI>();
    }

    public void DeductHealth(float deductHealth)
    {
        enemyHealth -= deductHealth;

        if(enemyHealth <= 0) { EnemyDead(); }
    }

    void EnemyDead()
    {
        enemyAI.EnemyDeathAnim();
        gameObject.SetActive(false);
        PhotonNetwork.Destroy(gameObject);
    }
}

