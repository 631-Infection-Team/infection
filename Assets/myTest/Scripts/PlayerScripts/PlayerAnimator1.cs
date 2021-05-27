using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
    

namespace myTest

{
    [RequireComponent(typeof(Player1))]
    [RequireComponent(typeof(PlayerCamera))]
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerAnimator1 : MonoBehaviourPun
    {
        
        [SerializeField] private Player1 player = null;
        [SerializeField] private Animator survivorAnimator = null;
       

        private Animator currentAnimator = null;

        public void Update()
        {
            if (!photonView.IsMine) return;
            if (!GetComponent<CharacterController>().enabled) return;
            SurvivorUpdate();
           
        }

        
     

        private void SurvivorUpdate()
        {
            currentAnimator.SetBool("Grounded", true);
            currentAnimator.SetFloat("Speed_f", 0f);
        }
   
    }

}
