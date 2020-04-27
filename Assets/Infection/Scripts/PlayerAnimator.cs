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
