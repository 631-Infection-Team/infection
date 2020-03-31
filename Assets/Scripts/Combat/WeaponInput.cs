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
        public bool LockControl = false;

        // Components
        private Weapon _weapon = null;

        // Properties
        private bool _fireDown = false;

        private void Awake()
        {
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

            // Reset fire when trigger is released
            if (Input.GetAxis("Fire") <= 0f)
            {
                _fireDown = false;
            }

            if (_weapon.CurrentWeapon.WeaponDefinition)
            {
                switch (_weapon.CurrentWeapon.WeaponDefinition.TriggerType)
                {
                    case TriggerType.Auto:
                        // Automatic fire is the same as burst
                    case TriggerType.Burst:
                        // Currently you can hold down Fire to fire burst mode weapons
                        if (Input.GetAxis("Fire") > 0f)
                        {
                            StartCoroutine(_weapon.FireWeapon());
                        }
                        break;

                    case TriggerType.Manual:
                        // Manual fire
                        if (Input.GetAxis("Fire") > 0f)
                        {
                            if (!_fireDown)
                            {
                                _fireDown = true;
                                StartCoroutine(_weapon.FireWeapon());
                            }
                        }
                        break;
                }

                // Reload weapon
                if (Input.GetButtonDown("Reload"))
                {
                    StartCoroutine(_weapon.ReloadWeapon());
                }

                // Aiming down the sights
                if (Input.GetAxis("Aim") > 0f)
                {
                    _weapon.IncreaseAim();
                }
                else
                {
                    _weapon.DecreaseAim();
                }
            }

            if (_weapon.HasMoreWeapons)
            {
                // Scroll up
                if (Input.GetAxis("Mouse ScrollWheel") > 0f || Input.GetButtonDown("Switch"))
                {
                    // Switch to the next weapon
                    _weapon.CycleWeapons();
                }
                // Scroll down
                else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
                {
                    // Switch to the previous weapon
                    _weapon.CycleWeapons(-1);
                }
            }
        }
    }
}
