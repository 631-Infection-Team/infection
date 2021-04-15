using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagement : MonoBehaviour
{

    public int round = 1;
    int zombiesInRound = 10;
    int zombiesSpawnedInRound = 0;
    float zombiesSpwanTimer = 0;
    public Transform[] zombieSpawnPoints;
    public GameObject zombieEnemy;


    void Update()
    {
     if(zombiesSpawnedInRound < zombiesInRound)
        {
            if(zombiesSpawnedInRound > 3)
            {
                SpawnZombie();
                zombiesSpwanTimer = 0;
            }
            else 
            {
                zombiesSpwanTimer+= Time.deltaTime;
            }
        }

     void SpawnZombie()
        {
            Vector3 randomSpwanPoint = zombieSpawnPoints[Random.Range(0, zombieSpawnPoints.Length)].position;
            Instantiate(zombieEnemy, randomSpwanPoint, Quaternion.identity);
            zombiesSpawnedInRound++;
        
        }
    }
}
