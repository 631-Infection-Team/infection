using Mirror;
using UnityEngine;

namespace Infection
{
    public class CameraController : NetworkBehaviour
    {
        public Camera currentCamera;
        public Camera weaponCamera;
        public bool LockControl;

        private float targetVerticalAngle;
        private PlayerController playerController;
        private Animator animator;

        public override void OnStartLocalPlayer()
        {
            playerController = GetComponent<PlayerController>();
            animator = playerController.playerModel.GetComponent<Animator>();

            if (currentCamera)
            {
                currentCamera.enabled = true;
            }

            if (weaponCamera)
            {
                weaponCamera.enabled = true;
            }
        }

        private void Update()
        {
            if (isLocalPlayer && currentCamera && playerController)
            {
                float lookY = Input.GetAxis("Mouse Y");

                if (!LockControl)
                {
                    targetVerticalAngle -= lookY;
                    if (targetVerticalAngle > 90f) targetVerticalAngle = 90f;
                    if (targetVerticalAngle < -90f) targetVerticalAngle = -90f;

                    if (animator)
                    {
                        animator.SetFloat("Head_Vertical_f", -(targetVerticalAngle / 90f));
                        animator.SetFloat("Body_Vertical_f", -(targetVerticalAngle / 90f) / 2);
                    }

                    Vector3 currentAngles = currentCamera.transform.localEulerAngles;
                    currentAngles.x = targetVerticalAngle;
                    currentAngles.z = Vector3.Dot(playerController.Velocity, -transform.right) / playerController.RunSpeed;

                    currentCamera.transform.localEulerAngles = currentAngles;
                }
            }
        }
    }
}
