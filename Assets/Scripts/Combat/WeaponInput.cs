using Mirror;
using UnityEngine;

namespace Infection.Combat
{
    [RequireComponent(typeof(Weapon))]
    public class WeaponInput : NetworkBehaviour
    {
        /// <summary>
        /// Prevent the player from controlling the weapon. Used for when the game is paused.
        /// </summary>
        public bool LockControl
        {
            get => _lockControl;
            set => _lockControl = value;
        }

        private Weapon _weapon = null;
        private bool _lockControl = false;

        public override void OnStartLocalPlayer()
        {
            if (!isLocalPlayer)
            {
                return;
            }

            _weapon = GetComponent<Weapon>();
        }

        private void Update()
        {
            if (!isLocalPlayer)
            {
                return;
            }

            if (LockControl)
            {
                return;
            }

            if (_weapon.CurrentWeapon.WeaponDefinition)
            {
                switch (_weapon.CurrentWeapon.WeaponDefinition.TriggerType)
                {
                    case TriggerType.Auto:
                        // Automatic fire is the same as burst
                    case TriggerType.Burst:
                        // Currently you can hold down Fire to fire burst mode weapons
                        if (Input.GetButton("Fire"))
                        {
                            StartCoroutine(_weapon.FireWeapon());
                        }
                        break;

                    case TriggerType.Manual:
                        // Manual fire
                        if (Input.GetButtonDown("Fire"))
                        {
                            StartCoroutine(_weapon.FireWeapon());
                        }
                        break;
                }

                // Reload weapon
                if (Input.GetButtonDown("Reload"))
                {
                    StartCoroutine(_weapon.ReloadWeapon());
                }

                // Aiming down the sights
                if (Input.GetButton("Aim"))
                {
                }
            }
        }
    }
}
