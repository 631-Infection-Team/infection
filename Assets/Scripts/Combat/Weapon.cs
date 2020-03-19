using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Infection.Combat
{
    public class Weapon : MonoBehaviour
    {
        [SerializeField] private WeaponDefinition currentWeapon = null;
        [SerializeField] private WeaponDefinition stowedWeapon = null;
        [SerializeField] private float range = 100f;

        private CameraController m_CameraController = null;
        private WeaponState currentState = WeaponState.Idle;
        private bool aimingDownSights = false;
        private int magazine = 0;
        private int reserves = 0;
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

            if (currentWeapon)
            {
                magazine = currentWeapon.ClipSize;
                reserves = currentWeapon.MaxReserves;
            }
        }

        private void Update()
        {
            if (currentWeapon)
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

        /// <summary>
        /// Fire the currently equipped weapon.
        /// </summary>
        /// <returns>Firing state</returns>
        private IEnumerator FireWeapon()
        {
            // Cannot fire weapon when state is not idle
            if (currentState != WeaponState.Idle)
            {
                yield break;
            }

            // Out of ammo
            if (magazine <= 0)
            {
                if (reserves <= 0)
                {
                    Debug.Log("Out of ammo!");
                    // TODO: Handle the case. Maybe switch weapon?
                    yield break;
                }

                StartCoroutine(ReloadWeapon());
            }

            // Fire the weapon
            currentState = WeaponState.Firing;
            if (Physics.Raycast(m_CameraController.currentCamera.transform.position, m_CameraController.currentCamera.transform.forward, out var hit, range))
            {
                Debug.Log(currentWeapon.WeaponName + " hit target " + hit.transform.name);
            }

            // Subtract ammo and wait for next shot
            magazine--;
            yield return new WaitForSeconds(currentWeapon.FireRate);
            currentState = WeaponState.Idle;
        }

        /// <summary>
        /// Reloads currently equipped weapon.
        /// </summary>
        /// <returns>Reload state</returns>
        private IEnumerator ReloadWeapon()
        {
            // Already reloading or not in idle state
            if (currentState == WeaponState.Reloading || currentState != WeaponState.Idle)
            {
                yield break;
            }

            // No more ammo
            if (reserves <= 0)
            {
                Debug.Log("No more ammo in reserves!");
                // TODO: Display a message in the HUD to indicate that the player has no more ammo
                yield break;
            }

            // Weapon already fully reloaded
            if (magazine >= currentWeapon.ClipSize)
            {
                Debug.Log("Magazine fully loaded, no need to reload.");
                // TODO: Display a message in the HUD to indicate that the magazine is already filled up
                yield break;
            }

            // Reloading animation
            currentState = WeaponState.Reloading;
            // TODO: Play animation
            yield return new WaitForSeconds(currentWeapon.ReloadTime);
            currentState = WeaponState.Idle;

            // Fill up magazine with ammo from reserves
            int ammoToAdd = reserves -= currentWeapon.ClipSize - magazine;
            magazine += ammoToAdd;
        }

        /// <summary>
        /// Switch current weapon to stowed weapon.
        /// </summary>
        /// <param name="weapon">New weapon to equip</param>
        /// <returns>Switching state</returns>
        private IEnumerator SwitchWeapon(WeaponDefinition weapon)
        {
            // Cannot switch weapon not in idle state
            if (currentState != WeaponState.Idle)
            {
                yield break;
            }

            // Begin switching weapons
            currentState = WeaponState.Switching;
            // TODO: Play putting away weapon animation
            yield return new WaitForSeconds(currentWeapon.HolsterTime);

            stowedWeapon = currentWeapon;
            currentWeapon = weapon;
            // TODO: Play pulling out weapon animation
            yield return new WaitForSeconds(weapon.ReadyTime);
            currentState = WeaponState.Idle;
        }
    }
}
