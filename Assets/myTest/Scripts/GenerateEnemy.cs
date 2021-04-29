using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

namespace myTest
{
    public class GenerateEnemy : MonoBehaviourPun
    {

        public GameObject theEnemy;
        public int xPos;
        public int zPos;
        public int enemyCount;
        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(EnemyDrop());
        }

        IEnumerator EnemyDrop()
        {
            while (enemyCount < 10)
            {
                xPos = Random.Range(1, 50);
                zPos = Random.Range(1, 31);
                //if (enemyCount % 2 == 0)
                //{
                //    PhotonNetwork.Instantiate(Path.Combine("PhotonZombie", "Zombie2_WithItemAnimatorsPhoton"), new Vector3(xPos, 0, zPos), Quaternion.identity);
                //}
                //
                //       I deleted the prefab before i put it in the prefab D:
                //else 
                //{
                //    PhotonNetwork.Instantiate(Path.Combine("PhotonZombie", "Zombie4_WithItemAnimators"), new Vector3(xPos, 0, zPos), Quaternion.identity);

                //}


                yield return new WaitForSeconds(0.1f);
                enemyCount += 1;
            }
        }
    }
}