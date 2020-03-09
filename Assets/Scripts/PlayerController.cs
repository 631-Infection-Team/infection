using Mirror;
using UnityEngine;

namespace Infection
{
    public class PlayerController : NetworkBehaviour
    {
        private CharacterController characterController;
        [Header("Movement")]
        [SerializeField]
        private float speed = 6.0f;
        [SerializeField]
        private float jumpSpeed = 1.0f;
        private Vector3 moveDirection = Vector3.zero;
        private float horizontal;
        private float vertical;

        private void Start()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (!isLocalPlayer)
            {
                return;
            }

            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
        }

        private void FixedUpdate()
        {
            if (!isLocalPlayer)
            {
                return;
            }

            if (characterController.isGrounded)
            {
                moveDirection = gameObject.transform.right * horizontal * speed + gameObject.transform.forward * vertical * speed;

                if (Input.GetButton("Jump"))
                {
                    moveDirection.y = jumpSpeed;
                }
            }

            // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
            // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
            // as an acceleration (ms^-2)
            moveDirection.y += Physics.gravity.y * Time.fixedDeltaTime;
            characterController.Move(moveDirection * Time.fixedDeltaTime);
        }
    }
}
