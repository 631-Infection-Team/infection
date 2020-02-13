using Mirror;
using UnityEngine;

namespace Infection
{
    public class Player : NetworkBehaviour
    {
        CharacterController characterController;

        [Header("Movement")]
        public float speed = 6.0f;

        void Start()
        {
            characterController = GetComponent<CharacterController>();
        }

        void Update()
        {
            if (!isLocalPlayer) return;

            Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), Physics.gravity.y * Time.deltaTime, Input.GetAxis("Vertical")) * speed;
            characterController.Move(moveDirection * Time.deltaTime);
        }
    }
}