using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Infection.Combat
{
    public class Weapon : MonoBehaviour
    {
        public enum WeaponState
        {
            Idle,
            Firing,
            Reloading,
            Switching
        }

        [SerializeField] private WeaponItem[] heldWeapons = new WeaponItem[2];
        [SerializeField] private float raycastRange = 100f;

        // Unity events. Add listeners from the inspector.
        [Header("Events for weapon behavior changes"), Tooltip("You may these to trigger sound effects")]
        [SerializeField] private UnityEvent onEquip = null;
        [SerializeField] private UnityEvent onFire = null;
        [SerializeField] private UnityEvent onReload = null;
        [SerializeField] private UnityEvent onSwitch = null;
        [SerializeField] private UnityEvent onReplace = null;

        /// <summary>
        /// Current state of the weapon. This can be idle, firing, reloading, or switching.
        /// </summary>
        public WeaponState CurrentState => currentState;

        /// <summary>
        /// The weapon currently in use. Returns one weapon item from the player's held weapons.
        /// </summary>
        public WeaponItem CurrentWeapon
        {
            get => heldWeapons[currentWeaponIndex];
            private set => heldWeapons[currentWeaponIndex] = value;
        }

        /// <summary>
        /// The player cannot hold additional weapons as their held weapons array is full.
        /// </summary>
        public bool IsFullOfWeapons => !Array.Exists(heldWeapons, w => w == null);

        // Events. Listeners added through code. The HUD script listens to these events to update the weapon display.
        public event Action OnAmmoChange = null;
        public event Action OnWeaponChange = null;
        public event OnAlert OnAlertEvent = null;
        public delegate IEnumerator OnAlert(string message, float duration);

        private CameraController m_CameraController = null;
        private int currentWeaponIndex = 0;
        private WeaponState currentState = WeaponState.Idle;
        private bool aimingDownSights = false;

        private void Awake()
        {
            m_CameraController = GetComponent<CameraController>();
        }

        private void Start()
        {
            // This is only to remind you that if this weapon script is attached to a player, it also needs weapon input.
            if (GetComponent<WeaponInput>() == null)
            {
                Debug.LogError("Weapon component does not work on its own and may require WeaponInput if used for the player.");
            }
        }

        /// <summary>
        /// Equip a new weapon in an empty slot. If there are no empty slots, replace currently equipped weapon.
        /// </summary>
        /// <param name="newWeapon">New weapon to equip</param>
        public void EquipWeapon(WeaponItem newWeapon)
        {
            // Player has no weapons
            // TODO: Find solution for edge case where player has other weapons but current weapon is null
            if (CurrentWeapon == null)
            {
                CurrentWeapon = newWeapon;
                OnWeaponChange?.Invoke();
                onEquip?.Invoke();
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
                WeaponItem old = ReplaceWeapon(currentWeaponIndex, newWeapon);
                Debug.Log("Replaced " + old.WeaponDefinition.WeaponName + " with " + newWeapon.WeaponDefinition.WeaponName);
            }

            onEquip?.Invoke();
        }

        /// <summary>
        /// Replaces a weapon from inventory at an index with a new weapon.
        /// </summary>
        /// <param name="index">Index of weapon inventory</param>
        /// <param name="newWeapon">New weapon to replace old</param>
        /// <returns>Old weapon that was replaced</returns>
        public WeaponItem ReplaceWeapon(int index, WeaponItem newWeapon)
        {
            WeaponItem oldWeapon = CurrentWeapon;
            CurrentWeapon = newWeapon;

            // Update listeners
            OnWeaponChange?.Invoke();
            onReplace?.Invoke();

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
            if (CurrentWeapon.Magazine <= 0)
            {
                if (CurrentWeapon.Reserves <= 0)
                {
                    Debug.Log("Out of ammo!");
                    StartCoroutine(OnAlertEvent?.Invoke("Out of ammo", 2f));
                    // Switch to a different weapon if it exists and if it still has ammo left
                    int nextWeapon = Array.FindIndex(heldWeapons, w => w != null && w.Magazine + w.Reserves > 0);
                    if (nextWeapon > -1)
                    {
                        StartCoroutine(SwitchWeapon(nextWeapon));
                    }
                    yield break;
                }

                StartCoroutine(ReloadWeapon());
                yield break;
            }

            // Firing burst type weapon
            if (CurrentWeapon.WeaponDefinition.TriggerType == TriggerType.Burst)
            {
                int burst = 3;
                for (int i = 0; i < burst && CurrentWeapon.Magazine > 0; i++)
                {
                    // Fire the weapon
                    currentState = WeaponState.Firing;
                    Fire();
                    // Wait a third of the fire rate between each shot in the burst
                    yield return new WaitForSeconds(CurrentWeapon.WeaponDefinition.FireRate / 3.0f);
                }

                // Wait twice as long between bursts
                yield return new WaitForSeconds(CurrentWeapon.WeaponDefinition.FireRate * 2.0f);
            }
            else
            {
                // Firing automatic or manual type weapon
                // Fire the weapon
                currentState = WeaponState.Firing;
                Fire();
                yield return new WaitForSeconds(CurrentWeapon.WeaponDefinition.FireRate);
            }

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
            if (CurrentWeapon.Reserves <= 0)
            {
                Debug.Log("No more ammo in reserves!");
                StartCoroutine(OnAlertEvent?.Invoke("Out of ammo", 1f));
                yield break;
            }

            // Weapon already fully reloaded
            if (CurrentWeapon.Magazine >= CurrentWeapon.WeaponDefinition.ClipSize)
            {
                Debug.Log("Magazine fully loaded, no need to reload.");
                StartCoroutine(OnAlertEvent?.Invoke("Magazine full", 1f));
                yield break;
            }

            // Reloading animation
            currentState = WeaponState.Reloading;
            // TODO: Play animation
            yield return new WaitForSeconds(CurrentWeapon.WeaponDefinition.ReloadTime);

            // Fill up magazine with ammo from reserves
            CurrentWeapon.ReloadMagazine();

            // Update listeners
            OnAmmoChange?.Invoke();
            onReload?.Invoke();

            currentState = WeaponState.Idle;
        }

        /// <summary>
        /// Switch current weapon to another held weapon by index.
        /// </summary>
        /// <param name="index">Index of weapon to switch to</param>
        /// <returns>Switching state</returns>
        public IEnumerator SwitchWeapon(int index)
        {
            // Cannot switch weapon when not in idle state or current weapon already out
            if (currentState != WeaponState.Idle || currentWeaponIndex == index)
            {
                yield break;
            }

            // Begin switching weapons
            currentState = WeaponState.Switching;
            Debug.Log("Switching weapon");
            // TODO: Play putting away weapon animation
            yield return new WaitForSeconds(CurrentWeapon.WeaponDefinition.HolsterTime);

            // Change the weapon
            currentWeaponIndex = index;

            // Update listeners
            OnAmmoChange?.Invoke();
            OnWeaponChange?.Invoke();
            onSwitch?.Invoke();

            // TODO: Play pulling out weapon animation
            yield return new WaitForSeconds(CurrentWeapon.WeaponDefinition.ReadyTime);
            Debug.Log("Weapon switch done");
            currentState = WeaponState.Idle;
        }

        /// <summary>
        /// Fire the weapon. Waiting for weapon state is not handled here.
        /// This method is only used to raycast and consume ammo.
        /// </summary>
        private void Fire()
        {
            switch (CurrentWeapon.WeaponDefinition.WeaponType)
            {
                case WeaponType.Raycast:
                    if (m_CameraController && Physics.Raycast(m_CameraController.currentCamera.transform.position, m_CameraController.currentCamera.transform.forward, out var hit, raycastRange))
                    {
                        Debug.Log(CurrentWeapon.WeaponDefinition.WeaponName + " hit target " + hit.transform.name);
                        Debug.DrawRay(m_CameraController.currentCamera.transform.position, m_CameraController.currentCamera.transform.forward, Color.red, 0.5f);
                    }
                    break;

                case WeaponType.Projectile:
                    // TODO: Implement projectile weapon firing
                    break;
            }

            // Subtract ammo
            CurrentWeapon.ConsumeMagazine(1);

            // Update listeners
            OnAmmoChange?.Invoke();
            onFire?.Invoke();
        }
    }
}
