using Mirror;
using System.Collections;
using UnityEngine;
using FMODUnity;

namespace Infection
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(StudioEventEmitter))]
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
        public Animator animator = null;
        public GameObject model = null;
        public GameObject HUD = null;
        public GameObject InfectedPlayer = null;
        public GameObject bloodImpactVfx = null;

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

        [Header("Gameplay")]
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
            gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            model.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

            horizontalLook = gameObject.transform.localEulerAngles.y;
            Cursor.lockState = CursorLockMode.Locked;
            gameObject.transform.position = NetworkManager.singleton.GetStartPosition().position;
        }

        private void Start()
        {
            characterController = gameObject.GetComponent<CharacterController>();
            animator = gameObject.GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            if (isClient && isLocalPlayer)
            {
                if (state != State.Dead)
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
            if (isClient && isLocalPlayer)
            {
                if (team == Team.Survivor)
                {
                    animator.SetFloat("Head_Vertical_f", -(verticalLook / 90f));
                    animator.SetFloat("Body_Vertical_f", -(verticalLook / 90f) / 2f);
                    animator.SetFloat("Speed_f", move.magnitude);
                    animator.SetBool("Death_b", state == State.Dead);
                    animator.SetBool("Grounded", state != State.Dead && isGrounded);
                }
            }
        }

        private void OnTriggerEnter(Collider col)
        {
            Trigger trigger = col.GetComponent<Trigger>();

            if (trigger)
            {
                if (trigger.type == Trigger.Type.Kill)
                {
                    DealDamageTo(this, health);
                }
                else if (trigger.type == Trigger.Type.Fire)
                {
                    StartCoroutine(Fire());
                }
            }
        }

        private void OnTriggerExit(Collider col)
        {
            Trigger trigger = col.GetComponent<Trigger>();

            if (trigger)
            {
                if (trigger.type == Trigger.Type.Kill)
                {
                    DealDamageTo(this, health);
                }
                else if (trigger.type == Trigger.Type.Fire)
                {
                    StopCoroutine(Fire());
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

        private IEnumerator Fire()
        {
            DealDamageTo(this, 2f);
            yield return new WaitForSeconds(0.5f);
        }

        private void OnTakeDamage(float amount, RaycastHit hit = new RaycastHit(), Vector3 direction = new Vector3())
        {
            if (team == Team.Spectator) return;

            health = Mathf.Clamp(Mathf.RoundToInt(health - amount), 0, healthMax);
            direction = hit.point == Vector3.zero ? characterController.transform.forward : direction;
            hit.point = hit.point == Vector3.zero ? gameObject.transform.position : hit.point;
            GameObject particles = Instantiate(bloodImpactVfx, hit.point, Quaternion.LookRotation(Vector3.Reflect(direction, hit.normal)));
            NetworkServer.Spawn(particles);

            if (health <= 0)
            {
                state = State.Dead;

                if (team == Team.Survivor)
                {
                    NetworkServer.DestroyPlayerForConnection(connectionToClient);
                    GameObject infectedPlayer = Instantiate(InfectedPlayer);
                    NetworkServer.Spawn(infectedPlayer, connectionToClient);
                    NetworkServer.AddPlayerForConnection(connectionToClient, infectedPlayer);
                }

                // Respawn
                StopAllCoroutines();

                state = State.Idle;
                health = healthMax;

                RpcOnRespawn();
            }
        }

        public void DealDamageTo(Player victim, float amount)
        {
            if (CanAttack(victim))
            {
                victim.OnTakeDamage(amount);
                victim.RpcOnDamageReceived();
            }
        }

        [ClientRpc]
        public override void RpcOnDamageReceived()
        {
            if (isLocalPlayer)
            {
                HUD hud = HUD.GetComponent<HUD>();
                hud.SetHealth(health);
                hud.SetHealthMax(healthMax);

                if (health <= 0)
                {
                    canShoot = false;
                    canMove = false;
                    canInteract = false;
                    canLook = false;
                }
            }
        }

        [ClientRpc]
        public void RpcOnRespawn()
        {
            if (isLocalPlayer)
            {
                Transform spawnPoint = NetRoomManager.singleton.GetStartPosition();
                gameObject.transform.position = spawnPoint.position;

                HUD hud = HUD.GetComponent<HUD>();
                hud.SetHealth(health);
                hud.SetHealthMax(healthMax);

                canShoot = true;
                canMove = true;
                canInteract = true;
                canLook = true;
            }
        }

        [Client]
        private void GravityHandler()
        {
            verticalSpeed += Physics.gravity.y * Time.deltaTime;
            verticalSpeed = Mathf.Clamp(verticalSpeed, Physics.gravity.y * 2, JumpSpeed);

            Vector3 verticalMove = new Vector3(0, verticalSpeed * Time.deltaTime, 0);
            CollisionFlags flag = characterController.Move(verticalMove);

            if ((flag & CollisionFlags.Below) != 0) verticalSpeed = 0;
        }

        [Client]
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

        [Client]
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

        [Client]
        private void InputHandler()
        {
            bool pause = Input.GetKeyDown(KeyCode.Escape);
            bool devKey = Input.GetKey(KeyCode.K);

            if (devKey)
            {
                DealDamageTo(this, 1);
            }

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
