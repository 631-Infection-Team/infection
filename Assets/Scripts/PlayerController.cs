using Mirror;
using UnityEngine;

namespace Infection
{
    public class PlayerController : NetworkBehaviour
    {
        private CharacterController characterController;
        private Transform groundTrigger;
        private Vector3 velocity;
        private bool isGrounded;

        [Header("Movement")]
        [SerializeField]
        private float speed = 12.0f;

        void Start()
        {
            characterController = GetComponent<CharacterController>();
            groundTrigger = gameObject.transform.Find("Ground Trigger").transform;
        }

        void FixedUpdate()
        {
            if (!isLocalPlayer) return;

            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            isGrounded = Physics.CheckSphere(groundTrigger.position, 0.4f, LayerMask.GetMask("Ground"));
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            Vector3 moveDirection = gameObject.transform.right * horizontal + gameObject.transform.forward * vertical;
            characterController.Move(moveDirection * speed * Time.fixedDeltaTime);

            // Gravity
            velocity.y += Physics.gravity.y * Time.fixedDeltaTime;
            characterController.Move(velocity * Time.fixedDeltaTime);
        }
    }
}