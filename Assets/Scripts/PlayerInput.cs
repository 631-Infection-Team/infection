using Mirror;
using UnityEngine;

namespace Infection
{
    public class PlayerInput : NetworkBehaviour
    {
        [SerializeField] private CameraController cameraController;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private GameObject HUD;

        public bool LockControl;

        public override void OnStartLocalPlayer()
        {
            if (isLocalPlayer)
            {
                playerController.LockControl = false;
                cameraController.LockControl = false;
            }
        }

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

                    HUD.GetComponent<HUD>().TogglePause();
                }

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

        private void OnDestroy()
        {
            if (isLocalPlayer)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}