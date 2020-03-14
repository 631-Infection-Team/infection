using Mirror;
using UnityEngine;

namespace Infection
{
    public class PlayerController : NetworkBehaviour
    {
        private CharacterController characterController;

        [Header("Control Settings")]
        [SerializeField] private float PlayerSpeed = 5.0f;
        [SerializeField] private float RunningSpeed = 7.0f;
        [SerializeField] private float JumpSpeed = 5.0f;
        private float groundedTimer = 0.0f;
        private float speedAtJump = 0.0f;
        private float verticalSpeed = 0.0f;
        private float horizontalSpeed = 0.0f;

        public float Speed { get; private set; } = 0.0f;
        public bool LockControl { get; set; }
        public bool Grounded { get; private set; }

        [Client]
        private void Start()
        {
            if (isLocalPlayer)
            {
                Grounded = true;
                horizontalSpeed = transform.localEulerAngles.y;

                characterController = GetComponent<CharacterController>();
            }
        }

        [Client]
        private void Update()
        {
            if (isLocalPlayer)
            {
                float vertical = Input.GetAxis("Vertical");
                float horizontal = Input.GetAxis("Horizontal");
                float lookHorizontal = Input.GetAxis("Mouse X");
                bool jump = Input.GetButtonDown("Jump");
                bool run = Input.GetButton("Run");
                bool lostFooting = false;

                //we define our own grounded and not use the Character controller one as the character controller can flicker
                //between grounded/not grounded on small step and the like. So we actually make the controller "not grounded" only
                //if the character controller reported not being grounded for at least .5 second;
                if (!characterController.isGrounded)
                {
                    if (Grounded)
                    {
                        groundedTimer += Time.deltaTime;
                        if (groundedTimer >= 0.5f)
                        {
                            lostFooting = true;
                            Grounded = false;
                        }
                    }
                }
                else
                {
                    groundedTimer = 0.0f;
                    Grounded = true;
                }

                Speed = 0;
                Vector3 move = Vector3.zero;

                if (!LockControl)
                {
                    // Jumping
                    if (Grounded && jump)
                    {
                        verticalSpeed = JumpSpeed;
                        Grounded = false;
                        lostFooting = true;
                    }

                    float actualSpeed = run ? RunningSpeed : PlayerSpeed;

                    if (lostFooting)
                    {
                        speedAtJump = actualSpeed;
                    }

                    // Move around with WASD
                    move = new Vector3(horizontal, 0, vertical);

                    if (move.sqrMagnitude > 1.0f)
                    {
                        move.Normalize();
                    }

                    float usedSpeed = Grounded ? actualSpeed : speedAtJump;
                    move = move * usedSpeed * Time.deltaTime;
                    move = transform.TransformDirection(move);

                    characterController.Move(move);

                    float turnPlayer = lookHorizontal;
                    horizontalSpeed = horizontalSpeed + turnPlayer;
                    if (horizontalSpeed > 360) horizontalSpeed -= 360.0f;
                    if (horizontalSpeed < 0) horizontalSpeed += 360.0f;

                    Vector3 currentAngles = transform.localEulerAngles;
                    currentAngles.y = horizontalSpeed;

                    transform.localEulerAngles = currentAngles;
                    Speed = move.magnitude / (PlayerSpeed * Time.deltaTime);
                }

                verticalSpeed = verticalSpeed - 10.0f * Time.deltaTime;
                verticalSpeed = Mathf.Clamp(verticalSpeed, -10f, JumpSpeed);

                var verticalMove = new Vector3(0, verticalSpeed * Time.deltaTime, 0);
                var flag = characterController.Move(verticalMove);

                if ((flag & CollisionFlags.Below) != 0)
                {
                    verticalSpeed = 0;
                }
            }
        }
    }
}