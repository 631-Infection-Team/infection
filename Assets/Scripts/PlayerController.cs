using Mirror;
using UnityEngine;

namespace Infection
{
    public class PlayerController : NetworkBehaviour
    {
        CharacterController characterController;
        Transform groundTrigger;
        float groundDistance = 0.4f;
        Vector3 velocity;
        bool isGrounded;

        [Header("Movement")]
        public float speed = 12.0f;
        public LayerMask groundMask;

        private float horizontal;
        private float vertical;

        private void Start()
        {
            characterController = GetComponent<CharacterController>();
            groundTrigger = gameObject.transform.Find("Ground Trigger").transform;
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
            if (!isLocalPlayer || characterController == null)
            {
                return;
            }

            isGrounded = Physics.CheckSphere(groundTrigger.position, groundDistance, groundMask);
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;
            characterController.Move(moveDirection * speed * Time.deltaTime);

            // Gravity
            velocity.y += Physics.gravity.y * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
        }
    }
}
