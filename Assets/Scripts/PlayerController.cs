using Mirror;
using UnityEngine;

namespace Infection
{
    public class PlayerController : NetworkBehaviour
    {
        private CharacterController m_CharacterController;

        [Header("Control Settings")]
        public float PlayerSpeed = 5.0f;
        public float RunningSpeed = 7.0f;
        public float JumpSpeed = 5.0f;
        private float m_GroundedTimer = 0.0f;
        private float m_SpeedAtJump = 0.0f;
        private float m_VerticalSpeed = 0.0f;
        private float m_HorizontalAngle = 0.0f;

        public float Speed { get; private set; } = 0.0f;
        public bool LockControl { get; set; }
        public bool Grounded { get; private set; }

        [Client]
        private void Start()
        {
            if (isLocalPlayer)
            {
                Grounded = true;
                m_HorizontalAngle = transform.localEulerAngles.y;

                m_CharacterController = GetComponent<CharacterController>();
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
                bool run = Input.GetButtonDown("Run");
                bool lostFooting = false;

                //we define our own grounded and not use the Character controller one as the character controller can flicker
                //between grounded/not grounded on small step and the like. So we actually make the controller "not grounded" only
                //if the character controller reported not being grounded for at least .5 second;
                if (!m_CharacterController.isGrounded)
                {
                    if (Grounded)
                    {
                        m_GroundedTimer += Time.deltaTime;
                        if (m_GroundedTimer >= 0.5f)
                        {
                            lostFooting = true;
                            Grounded = false;
                        }
                    }
                }
                else
                {
                    m_GroundedTimer = 0.0f;
                    Grounded = true;
                }

                Speed = 0;
                Vector3 move = Vector3.zero;

                if (!LockControl)
                {
                    // Jumping
                    if (Grounded && jump)
                    {
                        m_VerticalSpeed = JumpSpeed;
                        Grounded = false;
                        lostFooting = true;
                    }

                    bool running = run;
                    float actualSpeed = running ? RunningSpeed : PlayerSpeed;

                    if (lostFooting)
                    {
                        m_SpeedAtJump = actualSpeed;
                    }

                    // Move around with WASD
                    move = new Vector3(horizontal, 0, vertical);

                    if (move.sqrMagnitude > 1.0f)
                    {
                        move.Normalize();
                    }

                    float usedSpeed = Grounded ? actualSpeed : m_SpeedAtJump;
                    move = move * usedSpeed * Time.deltaTime;
                    move = transform.TransformDirection(move);

                    m_CharacterController.Move(move);

                    float turnPlayer = lookHorizontal;
                    m_HorizontalAngle = m_HorizontalAngle + turnPlayer;
                    if (m_HorizontalAngle > 360) m_HorizontalAngle -= 360.0f;
                    if (m_HorizontalAngle < 0) m_HorizontalAngle += 360.0f;

                    Vector3 currentAngles = transform.localEulerAngles;
                    currentAngles.y = m_HorizontalAngle;

                    transform.localEulerAngles = currentAngles;
                    Speed = move.magnitude / (PlayerSpeed * Time.deltaTime);
                }

                m_VerticalSpeed = m_VerticalSpeed - 10.0f * Time.deltaTime;
                m_VerticalSpeed = Mathf.Clamp(m_VerticalSpeed, -10f, JumpSpeed);

                var verticalMove = new Vector3(0, m_VerticalSpeed * Time.deltaTime, 0);
                var flag = m_CharacterController.Move(verticalMove);

                if ((flag & CollisionFlags.Below) != 0)
                {
                    m_VerticalSpeed = 0;
                }
            }
        }
    }
}