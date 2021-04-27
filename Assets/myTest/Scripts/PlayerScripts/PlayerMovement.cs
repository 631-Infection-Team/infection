﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


namespace myTest
{

  
    public class PlayerMovement : MonoBehaviourPun
    {
   
        //public string footSteps;

        [Header("Components")]
       // public Player1 player;
        public CharacterController characterController;
        public PhotonView photonView;
        [Header("Movement")]
        [HideInInspector] public bool isGrounded = false;
        private readonly float moveSpeed = 10f;
        private readonly float jumpSpeed = 10f;
        private float lastGrounded;
        private float velocityAtJump;
        private float verticalVelocity;
        private float footTimer = 0.0f;
        private Vector3 moveDirection;
        float xRotation = 0f;
        public float mouseSensitivity = 100f;
        public Transform playerBody;


        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            photonView = GetComponent<PhotonView>();
        }
        //public void OnTriggerEnter(Collider other)
        //{
        //    Trigger trigger = other.GetComponent<Trigger>();
        //    if (trigger == null) return;

        //    if (trigger.type == Trigger.Type.Kill)
        //    {
        //        player.TakeDamage(player.health, other.GetComponent<NetworkIdentity>().netId);
        //    }
        //}

        private void Start()
        {
            if (!photonView.IsMine)
            {
                Destroy(GetComponentInChildren<Camera>().gameObject);
                Destroy(characterController);
            }
        }
        public void Update()
        {
            if (!photonView.IsMine) return;
         //   if (player.isDead) return;
            if (!characterController.enabled) return;

            Look();
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

            if(characterController.isGrounded && footTimer > 0.5f ){
                if(inputHorizontal > 0 || inputVertical > 0){
                    Debug.Log("The foot noise comith");
                  //  FMODUnity.RuntimeManager.PlayOneShot(footSteps);
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

        void Look()
        {
            float lookX = Input.GetAxis("Look X") * 100f * Time.deltaTime;
            float lookY = Input.GetAxis("Look Y") * 100f * Time.deltaTime;

            xRotation -= lookY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            playerBody.Rotate(Vector3.up * lookX);
        }
    }

}
