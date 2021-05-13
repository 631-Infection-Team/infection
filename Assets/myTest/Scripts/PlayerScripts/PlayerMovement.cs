using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;


namespace myTest
{


    public class PlayerMovement : MonoBehaviourPun
    {
        [FMODUnity.EventRef]
        public string footSteps;

        [Header("Components")]
        // public Player1 player;
        public CharacterController characterController;
        private PhotonView photonView;
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
        float yRotation = 0f;
        public float mouseSensitivity = 100f;
        public Transform playerBody;
        [SerializeField] GameObject CameraContainer;
        [SerializeField] GameObject playerCamera;
        [SerializeField] GameObject weaponCamera;
        [SerializeField] GameObject minimapCamera;
        [SerializeField] GameObject playerCanvas;
        [SerializeField] GameObject weapon;
        [SerializeField] Text playerName;
    //    [SerializeField] ParticleSystem gunFlash;
        public LayerMask layer;

        private void Awake()
        {
            string PlayerUserName = GetComponent<PhotonView>().Owner.NickName;
            playerName.text = PlayerUserName;
            characterController = GetComponent<CharacterController>();
            photonView = GetComponent<PhotonView>();
            //var coll = gunFlash.collision;
            //coll.enabled = true;

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
                //Destroy(playerCamera);
                //Destroy(weaponCamera);
                //Destroy(minimapCamera);
                Destroy(CameraContainer);
                Destroy(characterController);
                Destroy(playerCanvas);
                weapon.layer = 0;

            }
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        public void Update()
        {
            if (!photonView.IsMine) return;
            //   if (player.isDead) return;
            if (!characterController.enabled) return;
            if (Input.GetKeyDown("k"))
            {
                PhotonNetwork.Destroy(this.gameObject);
                GameObject.Find("GameManager").GetComponent<Match>().activeDeathUI();
            }
            Look();
           // if (Input.GetButtonDown("Fire")) { Shoot(); }
            InputHandler();
            GravityHandler();
            FootyNoise();
        }

        public void Shoot()
        {

           // gunFlash.Play();

            Vector3 forward = weaponCamera.transform.TransformDirection(Vector3.forward) * 2;
            //  Ray ray = Camera.main.ViewportPointToRay(forward);
            Debug.DrawRay(playerCamera.transform.position, forward, Color.green);
            RaycastHit hit;

            if (Physics.Raycast(weaponCamera.transform.position + forward, weaponCamera.transform.position + forward * 10, out hit))
            {
                Debug.Log(" a enemy ? " + hit.transform.gameObject.name);
                if (hit.collider.isTrigger == true)
                {

                    Debug.Log(" an enemy by trigger >>>> " + hit.transform.gameObject.name);
                    PhotonNetwork.Destroy(hit.transform.gameObject);
                }

                if (hit.transform.gameObject.name == ("Zombie1"))
                {
                    Debug.Log(" an enemy by name >>>> " + hit.transform.gameObject.name);
                    PhotonNetwork.Destroy(hit.transform.gameObject);

                }
                if (hit.transform.gameObject.name == ("Zombie2"))
                {
                    Debug.Log(" a enemy by name >>>> " + hit.transform.gameObject.name);
                    PhotonNetwork.Destroy(hit.transform.gameObject);

                }




            }

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

            if (characterController.isGrounded && footTimer > 0.5f)
            {
                if (inputHorizontal > 0 || inputVertical > 0)
                {
                    //    Debug.Log("The foot noise comith");
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

        void Look()
        {
            float lookX = Input.GetAxis("Look X") * 100f * Time.deltaTime;
            float lookY = Input.GetAxis("Look Y") * 100f * Time.deltaTime;

            xRotation -= lookY;
            xRotation = Mathf.Clamp(xRotation, -60f, 60f);

            yRotation += lookX;
            transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
            playerCamera.transform.localEulerAngles = new Vector3(xRotation, 0f, 0f); //(Vector3.up * lookX);

            //transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * mouseSensitivity);

            //verticalLookRotation += Input.GetAxis("Mouse Y") * mouseSensitivity;
            //verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);

            //cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation;

        }
    }

}
