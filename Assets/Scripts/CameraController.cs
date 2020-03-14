using Mirror;
using UnityEngine;

namespace Infection
{
    public class CameraController : NetworkBehaviour
    {
        private float verticalAngle;

        public Camera currentCamera;
        public Transform CameraParent;
        public bool LockControl { get; set; }

        [Client]
        private void Start()
        {
            if (isLocalPlayer)
            {
                currentCamera.gameObject.SetActive(true);
                currentCamera.transform.SetParent(CameraParent, false);
                currentCamera.transform.localPosition = Vector3.zero;
                currentCamera.transform.localRotation = Quaternion.identity;
            }
        }

        [Client]
        private void Update()
        {
            if (isLocalPlayer)
            {
                float vertical = Input.GetAxis("Mouse Y");

                if (!LockControl)
                {
                    verticalAngle -= vertical;
                    if (verticalAngle > 90f) verticalAngle = 90f;
                    if (verticalAngle < -90f) verticalAngle = -90f;

                    Vector3 currentAngles = currentCamera.transform.localEulerAngles;
                    currentAngles.x = verticalAngle;

                    currentCamera.transform.localEulerAngles = currentAngles;
                }
            }
        }
    }
}
