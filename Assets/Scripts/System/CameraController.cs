using Mirror;
using UnityEngine;

namespace Infection
{
    public class CameraController : NetworkBehaviour
    {
        public Camera currentCamera;
        public Transform CameraParent;

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

        }
    }
}
