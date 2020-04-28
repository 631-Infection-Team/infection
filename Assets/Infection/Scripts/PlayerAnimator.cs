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
        [SerializeField] private Player player;
        [SerializeField] private Animator survivorAnimator;
        [SerializeField] private Animator zombieAnimator;

        public Animator Animator => animator;
        private Animator animator;

        public void Start()
        {

        }

        public void Update()
        {
            if (!isLocalPlayer) return;

            animator = (player.team == Player.Team.SURVIVOR) ? survivorAnimator : zombieAnimator;
            if (animator == survivorAnimator) { SurvivorUpdate(); } else { ZombieUpdate(); }
        }

        [Command]
        public void CmdJump()
        {
            RpcOnJump();
        }

        [ClientRpc]
        private void RpcOnJump()
        {
            animator.SetBool("Jump_b", true);
            animator.SetBool("Grounded", false);
        }

        private void SurvivorUpdate()
        {
            animator.SetBool("Grounded", true);
            animator.SetFloat("Speed_f", 0f);
        }

        private void ZombieUpdate()
        {
            animator.SetBool("Grounded", true);
            animator.SetFloat("Speed_f", 1f);
        }
    }

}
