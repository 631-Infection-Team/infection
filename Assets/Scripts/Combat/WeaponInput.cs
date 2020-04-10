using Mirror;
using UnityEngine;

namespace Infection.Combat
{
    [RequireComponent(typeof(Weapon))]
    public class WeaponInput : NetworkBehaviour
    {
        // Components
        private Player _player = null;
        private Weapon _weapon = null;

        // Properties
        private bool _fireDown = false;

        private void Awake()
        {
            _player = GetComponent<Player>();
            _weapon = GetComponent<Weapon>();
        }

        private void Update()
        {
            if (!isLocalPlayer)
            {
                return;
            }

            if (!_player.canShoot)
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

                // Aiming down the sights, in-between is possible with gamepad trigger
                _weapon.SetAim(Input.GetAxis("Aim"));
            }

            if (_weapon.HasMoreWeapons)
            {
                // Scroll up XOR Gamepad Button 3
                if (Input.GetAxis("Mouse ScrollWheel") > 0f ^ Input.GetButtonDown("Switch"))
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
                else if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    // Switch to first weapon
                    StartCoroutine(_weapon.SwitchWeapon(0));
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    // Switch to second weapon
                    StartCoroutine(_weapon.SwitchWeapon(1));
                }
            }
        }
    }
}
