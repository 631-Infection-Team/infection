using System;
using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Infection.Combat
{
    public class Weapon : MonoBehaviour
    {
        [SerializeField] private WeaponSlot[] heldWeapons = new WeaponSlot[2];
        [SerializeField] private float range = 100f;

        public WeaponState CurrentState => currentState;
        public WeaponSlot CurrentWeapon => heldWeapons[currentWeaponIndex];

        private CameraController m_CameraController = null;
        private int currentWeaponIndex = 0;
        private WeaponState currentState = WeaponState.Idle;
        private bool aimingDownSights = false;
        private float timeSinceFire = Mathf.Infinity;

        public enum WeaponState
        {
            Idle,
            Firing,
            Reloading,
            Switching
        }

        private void Start()
        {
            m_CameraController = GetComponent<CameraController>();
        }

        private void Update()
        {
            if (heldWeapons[currentWeaponIndex].weapon)
            {
                // Automatic fire
                if (Input.GetButton("Fire"))
                {
                    StartCoroutine(FireWeapon());
                }

                // Reload weapon
                if (Input.GetButtonDown("Reload"))
                {
                    StartCoroutine(ReloadWeapon());
                }
            }
        }

        public void EquipWeapon(WeaponSlot newWeapon)
        {
            // Player has no weapons
            // TODO: Find a better solution to check this
            if (heldWeapons[0] == null && currentWeaponIndex == 0)
            {
                heldWeapons[0] = newWeapon;
                return;
            }

            // Player has an empty slot in inventory
            int emptySlot = Array.FindIndex(heldWeapons, w => w == null);
            if (emptySlot > -1)
            {
                // Equip the new weapon and switch to it
                heldWeapons[emptySlot] = newWeapon;
                StartCoroutine(SwitchWeapon((currentWeaponIndex + 1) % heldWeapons.Length));
            }
            else
            {
                // No more space in inventory, replace current weapon with new one
                ReplaceWeapon(currentWeaponIndex, newWeapon);
            }
        }

        /// <summary>
        /// Replaces a weapon from inventory at an index with a new weapon.
        /// </summary>
        /// <param name="index">Index of weapon inventory</param>
        /// <param name="newWeapon">New weapon to replace old</param>
        /// <returns>Old weapon that was replaced</returns>
        public WeaponSlot ReplaceWeapon(int index, WeaponSlot newWeapon)
        {
            WeaponSlot oldWeapon = heldWeapons[currentWeaponIndex];
            heldWeapons[currentWeaponIndex] = newWeapon;
            return oldWeapon;
        }

        /// <summary>
        /// Fire the currently equipped weapon.
        /// </summary>
        /// <returns>Firing state</returns>
        public IEnumerator FireWeapon()
        {
            // Cannot fire weapon when state is not idle
            if (currentState != WeaponState.Idle)
            {
                yield break;
            }

            // Out of ammo
            if (heldWeapons[currentWeaponIndex].magazine <= 0)
            {
                if (heldWeapons[currentWeaponIndex].reserves <= 0)
                {
                    // TODO: Display a message in the HUD to indicate that the player has no more ammo
                    Debug.Log("Out of ammo!");
                    // Switch to a weapon that still has ammo left if such weapon exists
                    int nextWeapon = Array.FindIndex(heldWeapons, w => w.magazine > 0 || w.reserves > 0);
                    if (nextWeapon >= -1)
                    {
                        StartCoroutine(SwitchWeapon(nextWeapon));
                    }
                    yield break;
                }

                StartCoroutine(ReloadWeapon());
            }

            // Fire the weapon
            currentState = WeaponState.Firing;
            if (m_CameraController && Physics.Raycast(m_CameraController.currentCamera.transform.position, m_CameraController.currentCamera.transform.forward, out var hit, range))
            {
                Debug.Log(heldWeapons[currentWeaponIndex].weapon.WeaponName + " hit target " + hit.transform.name);
            }

            // Subtract ammo and wait for next shot
            heldWeapons[currentWeaponIndex].magazine--;
            yield return new WaitForSeconds(heldWeapons[currentWeaponIndex].weapon.FireRate);
            currentState = WeaponState.Idle;
        }

        /// <summary>
        /// Reloads currently equipped weapon.
        /// </summary>
        /// <returns>Reload state</returns>
        public IEnumerator ReloadWeapon()
        {
            // Already reloading or not in idle state
            if (currentState == WeaponState.Reloading || currentState != WeaponState.Idle)
            {
                yield break;
            }

            // No more ammo
            if (heldWeapons[currentWeaponIndex].reserves <= 0)
            {
                Debug.Log("No more ammo in reserves!");
                // TODO: Display a message in the HUD to indicate that the player has no more ammo
                yield break;
            }

            // Weapon already fully reloaded
            if (heldWeapons[currentWeaponIndex].magazine >= heldWeapons[currentWeaponIndex].weapon.ClipSize)
            {
                Debug.Log("Magazine fully loaded, no need to reload.");
                // TODO: Display a message in the HUD to indicate that the magazine is already filled up
                yield break;
            }

            // Reloading animation
            currentState = WeaponState.Reloading;
            // TODO: Play animation
            yield return new WaitForSeconds(heldWeapons[currentWeaponIndex].weapon.ReloadTime);
            currentState = WeaponState.Idle;

            // Fill up magazine with ammo from reserves
            int ammoToAdd = heldWeapons[currentWeaponIndex].reserves -= heldWeapons[currentWeaponIndex].weapon.ClipSize - heldWeapons[currentWeaponIndex].magazine;
            heldWeapons[currentWeaponIndex].magazine += ammoToAdd;
        }

        /// <summary>
        /// Switch current weapon to another held weapon by index.
        /// </summary>
        /// <param name="index">Index of weapon to switch to</param>
        /// <returns>Switching state</returns>
        public IEnumerator SwitchWeapon(int index)
        {
            // Cannot switch weapon not in idle state
            if (currentState != WeaponState.Idle)
            {
                yield break;
            }

            // Begin switching weapons
            currentState = WeaponState.Switching;
            // TODO: Play putting away weapon animation
            yield return new WaitForSeconds(heldWeapons[currentWeaponIndex].weapon.HolsterTime);

            currentWeaponIndex = index;
            // TODO: Play pulling out weapon animation
            yield return new WaitForSeconds(heldWeapons[currentWeaponIndex].weapon.ReadyTime);
            currentState = WeaponState.Idle;
        }
    }
}
