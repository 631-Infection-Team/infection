using Mirror;
using UnityEngine;

namespace Infection
{
    public class CameraController : NetworkBehaviour
    {
        public Camera currentCamera;
        public bool LockControl;

        [SerializeField] private float viewbobTimer = 1.25f;
        [SerializeField] private float viewbobScale = 1.25f;
        private float targetVerticalAngle;
        //private float targetHorizontalAngle;
        private PlayerController playerController;

        [Client]
        private void Start()
        {
            if (isLocalPlayer)
            {
                playerController = GetComponent<PlayerController>();
            }
        }

        [Client]
        private void Update()
        {
            if (isLocalPlayer)
            {
                float lookY = Input.GetAxis("Mouse Y");
                //float lookX = Input.GetAxis("Mouse X");

                currentCamera.transform.position = transform.position;

                if (!LockControl)
                {
                    targetVerticalAngle -= lookY;
                    if (targetVerticalAngle > 90f) targetVerticalAngle = 90f;
                    if (targetVerticalAngle < -90f) targetVerticalAngle = -90f;

                    //targetHorizontalAngle += lookX;
                    //if (targetHorizontalAngle > 360) targetHorizontalAngle -= 360.0f;
                    //if (targetHorizontalAngle < 0) targetHorizontalAngle += 360.0f;

                    Vector3 currentAngles = currentCamera.transform.localEulerAngles;
                    currentAngles.x = targetVerticalAngle - Mathf.Cos(Time.time * viewbobTimer) * viewbobScale;
                    currentAngles.y = Mathf.Sin(Time.time * viewbobTimer) * viewbobScale;
                    currentAngles.z = Vector3.Dot(playerController.Velocity, -transform.right) / playerController.WalkSpeed;

                    currentCamera.transform.localEulerAngles = currentAngles;
                }
            }
        }
    }
}
