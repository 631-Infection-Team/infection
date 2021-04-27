using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using FMODUnity;

namespace myTest
{

  
    public class PlayerMovement : MonoBehaviourPun
    {
        [BankRef]
        public string footSteps;

        [Header("Components")]
        public Player1 player;
        public CharacterController characterController;

        [Header("Movement")]
        [HideInInspector] public bool isGrounded = false;
        private readonly float moveSpeed = 12f;
        private readonly float jumpSpeed = 10f;
        private float lastGrounded;
        private float velocityAtJump;
        private float verticalVelocity;
        private float footTimer = 0.0f;
        private Vector3 moveDirection;

    
        //public void OnTriggerEnter(Collider other)
        //{
        //    Trigger trigger = other.GetComponent<Trigger>();
        //    if (trigger == null) return;

        //    if (trigger.type == Trigger.Type.Kill)
        //    {
        //        player.TakeDamage(player.health, other.GetComponent<NetworkIdentity>().netId);
        //    }
        //}

        public void Update()
        {
            if (!photonView.IsMine) return;
            if (player.isDead) return;
            if (!characterController.enabled) return;

            InputHandler();
            GravityHandler();
            FootyNoise();
        }

       
        private void InputHandler()
        {
            float inputHorizontal = Input.GetAxis("Horizontal");
            float inputVertical = Input.GetAxis("Vertical");
            bool inputJump = Input.GetButtonDown("Jump");
            bool lostFooting = false;

            if (characterController.isGrounded)
            {
                lastGrounded = 0f;
                isGrounded = true;
            }
            else
            {
                if (isGrounded)
                {
                    lastGrounded += Time.deltaTime;

                    if (lastGrounded >= 0.5f)
                    {
                        lostFooting = true;
                        isGrounded = false;
                    }
                }
            }

            if (isGrounded && inputJump)
            {
                verticalVelocity = jumpSpeed;
                isGrounded = false;
                lostFooting = true;
            }

            if (lostFooting) velocityAtJump = moveSpeed;
            moveDirection = new Vector3(inputHorizontal, 0, inputVertical);
            if (moveDirection.magnitude > 1) moveDirection = moveDirection.normalized;

            float calcSpeed = isGrounded ? moveSpeed : velocityAtJump;
            moveDirection = moveDirection * calcSpeed * Time.deltaTime;
            moveDirection = transform.TransformDirection(moveDirection);

            characterController.Move(moveDirection);
        }

       
        private void FootyNoise()
        {
            float inputHorizontal = Input.GetAxis("Horizontal");
            float inputVertical = Input.GetAxis("Vertical");
            footTimer += Time.deltaTime;

            if(characterController.isGrounded && footTimer > 0.5f && !player.isDead){
                if(inputHorizontal > 0 || inputVertical > 0){
                    Debug.Log("The foot noise comith");
                    FMODUnity.RuntimeManager.PlayOneShot(footSteps);
                    footTimer = 0.0f;
                }
            }
        }

        private void GravityHandler()
        {
            verticalVelocity += Physics.gravity.y * Time.deltaTime;
            verticalVelocity = Mathf.Clamp(verticalVelocity, Physics.gravity.y * 2, jumpSpeed);

            Vector3 verticalMove = new Vector3(0, verticalVelocity * Time.deltaTime, 0);
            CollisionFlags flag = characterController.Move(verticalMove);

            if ((flag & CollisionFlags.Below) != 0) verticalVelocity = 0;
        }
    }

}
