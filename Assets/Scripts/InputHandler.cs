using Mirror;
using UnityEngine;

namespace Infection
{
    public class InputHandler : NetworkBehaviour
    {
        private CameraController cameraController;
        private PlayerController playerController;
        public bool LockControl { get; set; }

        [Client]
        private void Start()
        {
            if (isLocalPlayer)
            {
                cameraController = GetComponent<CameraController>();
                playerController = GetComponent<PlayerController>();
            }
        }

        [Client]
        private void Update()
        {
            if (isLocalPlayer)
            {
                bool escape = Input.GetKeyDown(KeyCode.Escape);

                if (escape)
                {
                    LockControl = !LockControl;
                    playerController.LockControl = LockControl;
                    cameraController.LockControl = LockControl;

                    if (LockControl)
                    {
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;
                    }
                    else
                    {
                        Cursor.lockState = CursorLockMode.Locked;
                        Cursor.visible = false;
                    }
                }
            }
        }
    }
}