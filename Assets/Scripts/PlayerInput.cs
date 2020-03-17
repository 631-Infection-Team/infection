using Mirror;
using UnityEngine;

namespace Infection
{
    public class PlayerInput : NetworkBehaviour
    {
        [SerializeField] private CameraController cameraController;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private GameObject HUD;

        public bool LockControl { get; set; }

        [Client]
        private void Start()
        {
            if (isLocalPlayer)
            {
                playerController.LockControl = false;
                cameraController.LockControl = false;
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
    }
}