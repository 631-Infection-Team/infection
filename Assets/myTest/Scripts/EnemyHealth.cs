 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{

    public float enemyHealth = 100f;
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
        Destroy(gameObject, 10);
    }
}

