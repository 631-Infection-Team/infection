 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace myTest{
public class PlayerHealth : MonoBehaviourPun
{

    //public float health = 3;

    [SerializeField] private Player1 player = null;
    private void Start()
    {
        player = GetComponent<Player1>();
        //player.SetDefaults();
    }
    



}

}
