using Mirror;
using System.Collections;
using UnityEngine;

namespace Infection
{
    [RequireComponent(typeof(CharacterController))]
    public class Player : Entity
    {
        public static Player localPlayer;
        public enum Team
        {
            Spectator,
            Survivor,
            Infected
        }

        public enum State
        {
            Idle,
            Moving,
            Dead
        }

        [Header("Components")]
        public Camera cam = null;
        public GameObject model = null;
        public GameObject HUD = null;
        public GameObject bloodImpactVfx = null;
        public Animator animator = null;

        [Header("Movement")]
        public float walkSpeed = 8f;
        public float runSpeed = 48f;
        public float JumpSpeed = 5f;
        public float verticalLook = 0f;
        public float horizontalLook = 0f;
        public bool canMove = true;
        public bool canShoot = true;
        public bool canLook = true;
        public bool canInteract = true;
        public bool isGrounded = false;

        [Header("Configuration")]
        [SyncVar] public Team team = Team.Survivor;
        [SyncVar] public State state = State.Idle;

        private CharacterController characterController;
        private Vector3 move;
        private float speedAtJump;
        private float verticalSpeed;
        private float lastGrounded;
        private bool isPaused;

        public override void OnStartLocalPlayer()
        {
            localPlayer = this;

            HUD.gameObject.SetActive(true);
            cam.gameObject.SetActive(true);
            model.gameObject.SetActive(false);

            horizontalLook = gameObject.transform.localEulerAngles.y;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Start()
        {
            characterController = gameObject.GetComponent<CharacterController>();
            animator = gameObject.GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            if (isLocalPlayer)
            {
                if (state == State.Dead)
                {
                    CmdRespawn();
                }
                else
                {
                    CameraHandler();
                    MovementHandler();
                    GravityHandler();
                }

                InputHandler();
            }
        }

        private void LateUpdate()
        {
            animator.SetFloat("Head_Vertical_f", -(verticalLook / 90f));
            animator.SetFloat("Body_Vertical_f", -(verticalLook / 90f) / 2f);
            animator.SetFloat("Speed_f", move.magnitude);
            animator.SetBool("Death_b", state == State.Dead);
            animator.SetBool("Grounded", true);
        }

        private void OnTriggerEnter(Collider col)
        {
            Zone zone = col.GetComponent<Zone>();

            if (col.isTrigger && zone)
            {
                if (zone.zoneType == Zone.ZoneTypes.Kill)
                {
                    DealDamageTo(this, health);
                }
            }
        }

        private void OnDestroy()
        {
            if (isLocalPlayer)
            {
                localPlayer = null;
            }
        }

        public void DealDamageTo(Player player, float amount)
        {
            if (CanAttack(player))
            {
                player.health -= Mathf.Clamp(Mathf.RoundToInt(amount), 0, healthMax);
                player.RpcOnDamageReceived(Mathf.Clamp(Mathf.RoundToInt(amount), 0, healthMax));

                if (player.health <= 0)
                {
                    player.state = State.Dead;
                }
            }
        }

        [ClientRpc]
        public override void RpcOnDamageReceived(int amount)
        {
            if (isLocalPlayer)
            {
                HUD hud = HUD.GetComponent<HUD>();
                hud.SetHealth(health);
                hud.SetHealthMax(healthMax);
            }
        }

        [ClientRpc]
        public void RpcOnRespawn()
        {
            if (isLocalPlayer)
            {
                Transform spawnPoint = NetRoomManager.netRoomManager.GetStartPosition();
                gameObject.transform.position = spawnPoint.position;

                HUD hud = HUD.GetComponent<HUD>();
                hud.SetHealth(health);
                hud.SetHealthMax(healthMax);
            }
        }

        [Command]
        public void CmdRespawn()
        {
            state = State.Idle;
            health = healthMax;

            RpcOnRespawn();
        }

        private void GravityHandler()
        {
            verticalSpeed += Physics.gravity.y * Time.deltaTime;
            verticalSpeed = Mathf.Clamp(verticalSpeed, Physics.gravity.y * 2, JumpSpeed);

            Vector3 verticalMove = new Vector3(0, verticalSpeed * Time.deltaTime, 0);
            CollisionFlags flag = characterController.Move(verticalMove);

            if ((flag & CollisionFlags.Below) != 0) verticalSpeed = 0;
        }

        private void MovementHandler()
        {
            float inputHorizontal = Input.GetAxis("Horizontal");
            float inputVertical = Input.GetAxis("Vertical");
            bool inputJump = Input.GetButtonDown("Jump");
            bool inputRun = Input.GetButton("Run");
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

            if (canMove)
            {
                if (isGrounded && inputJump)
                {
                    verticalSpeed = JumpSpeed;
                    isGrounded = false;
                    lostFooting = true;
                }

                float actualSpeed = inputRun ? runSpeed : walkSpeed;
                if (lostFooting) speedAtJump = actualSpeed;

                move = new Vector3(inputHorizontal, 0, inputVertical);
                if (move.magnitude > 1) move = move.normalized;

                float calcSpeed = isGrounded ? actualSpeed : speedAtJump;
                move = move * calcSpeed * Time.deltaTime;
                move = transform.TransformDirection(move);

                characterController.Move(move);
            }
        }

        private void CameraHandler()
        {
            if (canLook)
            {
                float lookY = Input.GetAxis("Look Y");
                float lookX = Input.GetAxis("Look X");

                verticalLook -= lookY;
                if (verticalLook > 90f) verticalLook = 90f;
                if (verticalLook < -90f) verticalLook = -90f;

                Vector3 currentAngles = cam.transform.localEulerAngles;
                currentAngles.x = verticalLook;
                // currentAngles.z = Vector3.Dot(characterController.velocity, -transform.right);

                cam.transform.localEulerAngles = currentAngles;

                horizontalLook += lookX;
                if (horizontalLook > 360) horizontalLook -= 360.0f;
                if (horizontalLook < 0) horizontalLook += 360.0f;

                currentAngles = transform.localEulerAngles;
                currentAngles.y = horizontalLook;
                transform.localEulerAngles = currentAngles;
            }
        }

        private void InputHandler()
        {
            bool pause = Input.GetKeyDown(KeyCode.Escape);

            if (pause)
            {
                isPaused = !isPaused;
                canMove = !isPaused;
                canShoot = !isPaused;
                canLook = !isPaused;
                canInteract = !isPaused;
                Cursor.visible = isPaused;

                HUD.GetComponent<HUD>().SetPaused(isPaused);
            }

            Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }
}
