using Mirror;
using UnityEngine;

namespace Infection
{
    public class CameraController : NetworkBehaviour
    {
        private float targetVerticalAngle;
        private float targetHorizontalAngle;

        public Camera currentCamera;
        public bool LockControl { get; set; }

        [Client]

        private void Start()
        {
            if (isLocalPlayer)
            {
                currentCamera = Camera.main;
            }
        }

        [Client]
        private void Update()
        {
            if (isLocalPlayer)
            {
                float vertical = Input.GetAxis("Mouse Y");
                float horizontal = Input.GetAxis("Mouse X");

                currentCamera.transform.position = transform.position;

                if (!LockControl)
                {
                    targetVerticalAngle -= vertical;
                    if (targetVerticalAngle > 90f) targetVerticalAngle = 90f;
                    if (targetVerticalAngle < -90f) targetVerticalAngle = -90f;

                    targetHorizontalAngle += horizontal;
                    if (targetHorizontalAngle > 360) targetHorizontalAngle -= 360.0f;
                    if (targetHorizontalAngle < 0) targetHorizontalAngle += 360.0f;

                    Vector3 currentAngles = currentCamera.transform.localEulerAngles;
                    currentAngles.x = targetVerticalAngle;
                    currentAngles.y = targetHorizontalAngle;

                    currentCamera.transform.localEulerAngles = currentAngles;
                }
            }
        }
    }
}
