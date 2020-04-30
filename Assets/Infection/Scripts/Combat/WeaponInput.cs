using System.Collections;
using Mirror;
using UnityEngine;

namespace Infection.Combat
{
    public class WeaponInput : NetworkBehaviour
    {
        // Components
        public Player player = null;
        public Weapon weapon = null;

        // Properties
        private bool _fireDown = false;

        private void Update()
        {
            if (!isLocalPlayer)
            {
                return;
            }

            // Reset fire when trigger is released
            if (Input.GetAxis("Fire") <= 0f)
            {
                _fireDown = false;
            }

            if (weapon.CurrentWeapon != null && weapon.CurrentWeapon.weaponDefinition)
            {
                switch (weapon.CurrentWeapon.weaponDefinition.triggerType)
                {
                    case TriggerType.Auto:
                    // Automatic fire is the same as burst
                    case TriggerType.Burst:
                        // Currently you can hold down Fire to fire burst mode weapons
                        if (Input.GetAxis("Fire") > 0f)
                        {
                            StartCoroutine(weapon.FireWeapon());
                        }
                        break;

                    case TriggerType.Manual:
                        // Manual fire
                        if (Input.GetAxis("Fire") > 0f)
                        {
                            if (!_fireDown)
                            {
                                _fireDown = true;
                                StartCoroutine(weapon.FireWeapon());
                            }
                        }
                        break;
                }

                // Reload weapon
                if (Input.GetButtonDown("Reload"))
                {
                    StartCoroutine(weapon.ReloadWeapon());
                }

                // Aiming down the sights, in-between is possible with gamepad trigger
                weapon.SetAim(Input.GetAxis("Aim"));
            }

            if (weapon.HasMoreWeapons)
            {
                // Scroll up XOR Gamepad Button 3
                if (Input.GetAxis("Mouse ScrollWheel") > 0f ^ Input.GetButtonDown("Switch"))
                {
                    // Switch to the next weapon
                    weapon.CycleWeapons();
                }
                // Scroll down
                else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
                {
                    // Switch to the previous weapon
                    weapon.CycleWeapons(-1);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    // Switch to first weapon
                    StartCoroutine(weapon.SwitchWeapon(0));
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    // Switch to second weapon
                    StartCoroutine(weapon.SwitchWeapon(1));
                }
            }
        }
    }
}
