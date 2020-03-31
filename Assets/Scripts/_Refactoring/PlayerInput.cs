using Infection.Combat;
using Mirror;
using UnityEngine;

namespace Infection
{
    public class PlayerInput : NetworkBehaviour
    {
        [SerializeField] private CameraController cameraController = null;
        [SerializeField] private PlayerController playerController = null;
        [SerializeField] private WeaponInput weaponInput = null;
        [SerializeField] private GameObject HUD = null;

        public bool LockControl;

        public override void OnStartLocalPlayer()
        {
            if (isLocalPlayer)
            {
                playerController.LockControl = false;
                cameraController.LockControl = false;
                weaponInput.LockControl = false;
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
                    weaponInput.LockControl = LockControl;

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