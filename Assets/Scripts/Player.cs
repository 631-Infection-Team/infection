using Mirror;
using System.Collections;
using UnityEngine;

namespace Infection
{
    [RequireComponent(typeof(CharacterController))]
    public class Player : Entity
    {
        public static Player localPlayer;

        [Header("Components")]
        public Camera cam = null;
        public GameObject model = null;
        public GameObject HUD = null;

        [Header("Movement")]
        public float walkSpeed = 8f;
        public float runSpeed = 12f;
        public float JumpSpeed = 5f;
        public bool canMove = true;
        public bool canShoot = true;
        public bool canLook = true;
        public bool canInteract = true;
        public bool isGrounded = false;
        [SyncVar] public float verticalLook;
        public float horizontalLook;

        [Header("Team")]
        [SyncVar] public Team team = Team.Survivor;

        [Header("Visual Effects")]
        public GameObject bloodImpactVfx = null;

        [Header("Balance")]
        public int respawnTime = 5;

        public enum Team
        {
            Spectator,
            Survivor,
            Infected
        }

        private CharacterController characterController;
        private Vector3 move;
        private float speedAtJump;
        private float verticalSpeed;
        private float lastGrounded;
        private bool isPaused;

        protected override void Awake()
        {
            base.Awake();

            Utils.InvokeMany(typeof(Player), this, "Awake_");
        }

        public override void OnStartLocalPlayer()
        {
            localPlayer = this;

            HUD.gameObject.SetActive(true);
            cam.gameObject.SetActive(true);
            model.gameObject.SetActive(false);

            isGrounded = true;
            verticalLook = 0.0f;
            horizontalLook = gameObject.transform.localEulerAngles.y;
            Cursor.lockState = CursorLockMode.Locked;

            Utils.InvokeMany(typeof(Player), this, "OnStartLocalPlayer_");
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            Utils.InvokeMany(typeof(Player), this, "OnStartServer_");
        }

        protected override void Start()
        {
            if (!isServer && !isClient) return;

            characterController = gameObject.GetComponent<CharacterController>();

            base.Start();

            Utils.InvokeMany(typeof(Player), this, "Start_");
        }

        private void LateUpdate()
        {
            // pass parameters to animation state machine
            // => passing the states directly is the most reliable way to avoid all
            //    kinds of glitches like movement sliding, attack twitching, etc.
            // => make sure to import all looping animations like idle/run/attack
            //    with 'loop time' enabled, otherwise the client might only play it
            //    once
            // => MOVING state is set to local IsMovement result directly. otherwise
            //    we would see animation latencies for rubberband movement if we
            //    have to wait for MOVING state to be received from the server

            UpdateAnimator();

            Utils.InvokeMany(typeof(Player), this, "LateUpdate_");
        }

        private void OnTriggerEnter(Collider col)
        {
            Zone zone = col.GetComponent<Zone>();

            if (col.isTrigger && zone)
            {
                if (zone.zoneType == Zone.ZoneTypes.Kill)
                {
                    Debug.Log("Hit a kill trigger.");

                    DealDamageTo(this, health);
                }
            }
        }

        private void UpdateAnimator()
        {
            animator.SetFloat("Head_Vertical_f", -(verticalLook / 90f));
            animator.SetFloat("Body_Vertical_f", -(verticalLook / 90f) / 2);

            animator.SetFloat("Speed_f", move.magnitude);
            animator.SetBool("Death_b", state == "DEAD");
            animator.SetBool("Grounded", true);
        }

        private void OnDestroy()
        {
            if (!isServer && !isClient) return;

            if (isLocalPlayer)
            {
                localPlayer = null;
            }

            Utils.InvokeMany(typeof(Player), this, "OnDestroy_");
        }

        public bool IsMoving()
        {
            return move != Vector3.zero;
        }

        // finite state machine events
        bool EventDied()
        {
            return health <= 0;
        }

        bool EventMoveStart()
        {
            return state != "MOVING" && IsMoving(); // only fire when started moving
        }

        bool EventMoveEnd()
        {
            return state == "MOVING" && !IsMoving(); // only fire when stopped moving
        }

        IEnumerator RespawnTimer()
        {
            yield return new WaitForSeconds(respawnTime);

            health = healthMax;
            state = "IDLE";

            RpcOnRespawn();
        }

        [ClientRpc]
        public void RpcOnRespawn()
        {
            Transform spawnPoint = NetRoomManager.netRoomManager.GetStartPosition();

            gameObject.transform.position = spawnPoint.position;
            state = "IDLE";
        }

        [Command]
        public void CmdRespawn()
        {
            StartCoroutine(RespawnTimer());
        }

        // finite state machine - server
        [Server]
        string UpdateServer_IDLE()
        {
            if (EventDied())
            {
                OnDeath();
                return "DEAD";
            }
            if (EventMoveStart())
            {
                return "MOVING";
            }
            if (EventMoveEnd()) { }

            return "IDLE";
        }

        [Server]
        string UpdateServer_MOVING()
        {
            if (EventDied())
            {
                OnDeath();
                return "DEAD";
            }
            if (EventMoveEnd())
            {
                return "IDLE";
            }
            if (EventMoveStart()) { }

            return "MOVING";
        }

        [Server]
        string UpdateServer_DEAD()
        {
            if (EventMoveStart()) { }
            if (EventMoveEnd()) { }
            if (EventDied()) { }

            return "DEAD";
        }

        [Server]
        protected override string UpdateServer()
        {
            if (state == "IDLE") return UpdateServer_IDLE();
            if (state == "MOVING") return UpdateServer_MOVING();
            if (state == "DEAD") return UpdateServer_DEAD();

            Debug.LogError("invalid state:" + state);
            return "IDLE";
        }

        [Server]
        protected override void OnDeath()
        {
            base.OnDeath();
        }

        // finite state machine - client
        [Client]
        protected override void UpdateClient()
        {
            if (isLocalPlayer)
            {
                if (state != "DEAD")
                {
                    CameraHandler();
                    MovementHandler();
                }
                else if (state == "DEAD")
                {
                    CmdRespawn();
                }
                else
                {
                    Debug.LogError("invalid state:" + state);
                }

                InputHandler();
                GravityHandler();
            }

            Utils.InvokeMany(typeof(Player), this, "UpdateClient_");
        }

        public override bool CanAttack(Entity entity)
        {
            // Can only attack other players who aren't on the same team as you.
            // Disabled until we can test this easier.
            /**
            if (entity is Player)
            {
                Player victim = (Player)entity;
                return base.CanAttack(entity) && team != victim.team;
            }
            else
            {
                return base.CanAttack(entity) && entity;
            }
            **/

            return base.CanAttack(entity) && entity is Player;
        }

        [Client]
        void GravityHandler()
        {
            verticalSpeed += Physics.gravity.y * Time.deltaTime;
            verticalSpeed = Mathf.Clamp(verticalSpeed, Physics.gravity.y * 2, JumpSpeed);

            Vector3 verticalMove = new Vector3(0, verticalSpeed * Time.deltaTime, 0);
            CollisionFlags flag = characterController.Move(verticalMove);

            if ((flag & CollisionFlags.Below) != 0) verticalSpeed = 0;
        }

        [Client]
        void MovementHandler()
        {
            float inputHorizontal = Input.GetAxis("Horizontal");
            float inputVertical = Input.GetAxis("Vertical");
            bool inputJump = Input.GetButtonDown("Jump");
            bool inputRun = Input.GetButton("Run");
            bool lostFooting = false;

            if (!characterController.isGrounded)
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
            else
            {
                lastGrounded = 0f;
                isGrounded = true;
            }

            if (!canMove) return;

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

            // draw direction for debugging
            // Debug.DrawLine(transform.position, transform.position + direction, Color.green, 0, false);
        }

        [Client]
        void CameraHandler()
        {
            if (!canLook) return;

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

        [Client]
        void InputHandler()
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
