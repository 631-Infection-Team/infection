using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace Infection
{
    [RequireComponent(typeof(Player))]
    [RequireComponent(typeof(PlayerCamera))]
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerAnimator : NetworkBehaviour
    {
        /**
        [SerializeField] private Player player = null;
        [SerializeField] private Animator survivorAnimator = null;
        [SerializeField] private Animator zombieAnimator = null;

        private Animator currentAnimator = null;

        public void Update()
        {
            if (!isLocalPlayer) return;
            if (!GetComponent<CharacterController>().enabled) return;

            currentAnimator = (player.team == Player.Team.SURVIVOR) ? survivorAnimator : zombieAnimator;
            if (currentAnimator == survivorAnimator) { SurvivorUpdate(); } else { ZombieUpdate(); }
        }

        [Command]
        public void CmdJump()
        {
            RpcOnJump();
        }

        [ClientRpc]
        private void RpcOnJump()
        {
            currentAnimator.SetBool("Jump_b", true);
            currentAnimator.SetBool("Grounded", false);
        }

        private void SurvivorUpdate()
        {
            currentAnimator.SetBool("Grounded", true);
            currentAnimator.SetFloat("Speed_f", 0f);
        }

        private void ZombieUpdate()
        {
            currentAnimator.SetBool("Grounded", true);
            currentAnimator.SetFloat("Speed_f", 1f);
        }
    **/
    }

}
