using Mirror;
using UnityEngine;

namespace Infection
{
    [System.Serializable]
    public class Controller : NetworkBehaviour
    {
        [SerializeField] private Camera MainCamera;
        [SerializeField] private Transform CameraPosition;
        private CharacterController m_CharacterController;

        [Header("Control Settings")]
        public float PlayerSpeed = 5.0f;
        public float RunningSpeed = 7.0f;
        public float JumpSpeed = 5.0f;
        private float m_GroundedTimer = 0.0f;
        private float m_SpeedAtJump = 0.0f;
        private float m_VerticalSpeed = 0.0f;
        private float m_VerticalAngle = 0.0f;
        private float m_HorizontalAngle = 0.0f;

        public float Speed { get; private set; } = 0.0f;
        public bool LockControl { get; set; }
        public bool Grounded { get; private set; }

        private void Start()
        {
            if (isLocalPlayer)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                MainCamera.gameObject.SetActive(true);
                MainCamera.transform.SetParent(CameraPosition, false);
                MainCamera.transform.localPosition = Vector3.zero;
                MainCamera.transform.localRotation = Quaternion.identity;

                Grounded = true;

                m_CharacterController = GetComponent<CharacterController>();
                m_VerticalAngle = 0.0f;
                m_HorizontalAngle = transform.localEulerAngles.y;
            }
        }

        [Client]
        private void Update()
        {
            if (isLocalPlayer)
            {
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
                    if (Grounded && Input.GetButtonDown("Jump"))
                    {
                        m_VerticalSpeed = JumpSpeed;
                        Grounded = false;
                        lostFooting = true;
                    }

                    bool running = Input.GetButton("Run");
                    float actualSpeed = running ? RunningSpeed : PlayerSpeed;

                    if (lostFooting)
                    {
                        m_SpeedAtJump = actualSpeed;
                    }

                    // Move around with WASD
                    move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

                    if (move.sqrMagnitude > 1.0f)
                    {
                        move.Normalize();
                    }

                    float usedSpeed = Grounded ? actualSpeed : m_SpeedAtJump;
                    move = move * usedSpeed * Time.deltaTime;
                    move = transform.TransformDirection(move);

                    m_CharacterController.Move(move);

                    // Turn player
                    float turnPlayer = Input.GetAxis("Mouse X");
                    m_HorizontalAngle = m_HorizontalAngle + turnPlayer;
                    if (m_HorizontalAngle > 360) m_HorizontalAngle -= 360.0f;
                    if (m_HorizontalAngle < 0) m_HorizontalAngle += 360.0f;

                    Vector3 currentAngles = transform.localEulerAngles;
                    currentAngles.y = m_HorizontalAngle;
                    transform.localEulerAngles = currentAngles;

                    // Camera look up/down
                    m_VerticalAngle = Mathf.Clamp(-Input.GetAxis("Mouse Y") + m_VerticalAngle, -89.0f, 89.0f);
                    currentAngles = CameraPosition.transform.localEulerAngles;
                    currentAngles.x = m_VerticalAngle;

                    CameraPosition.transform.localEulerAngles = currentAngles;
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