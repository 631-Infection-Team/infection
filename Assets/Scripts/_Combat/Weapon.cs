using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Mirror;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Infection.Combat
{
    public class Weapon : NetworkBehaviour
    {
        public enum WeaponState
        {
            Idle,
            Firing,
            Reloading,
            Switching
        }

        [Header("Weapon")]
        [SerializeField] private WeaponItem[] heldWeapons = new WeaponItem[2];
        [SerializeField] private float raycastRange = 100f;
        [SerializeField] private LayerMask raycastMask = 0;

        [Header("Transforms for weapon model")]
        [SerializeField] private Transform weaponHolder = null;
        [SerializeField] private Transform muzzle = null;
        [SerializeField] private Transform muzzleFlash = null;

        // Unity events. Add listeners from the inspector.
        [Header("Events for weapon behavior state changes")]
        [SerializeField] private UnityEvent onEquip = null;
        [SerializeField] private UnityEvent onFire = null;
        [SerializeField] private UnityEvent onReload = null;
        [SerializeField] private UnityEvent onSwitch = null;
        [SerializeField] private UnityEvent onReplace = null;

        /// <summary>
        /// Current state of the weapon. This can be idle, firing, reloading, or switching.
        /// </summary>
        public WeaponState CurrentState
        {
            get => _currentState;
            set
            {
                _currentState = value;
                OnStateChange?.Invoke(this, new StateChangedEventArgs
                {
                    State = _currentState
                });
            }
        }

        /// <summary>
        /// The weapon currently in use. Returns one weapon item from the player's held weapons.
        /// </summary>
        public WeaponItem CurrentWeapon
        {
            get => heldWeapons[_currentWeaponIndex];
            private set => heldWeapons[_currentWeaponIndex] = value;
        }

        /// <summary>
        /// The player cannot hold additional weapons as their held weapons array is full.
        /// </summary>
        public bool IsFullOfWeapons => !Array.Exists(heldWeapons, w => w == null);

        // Events. Listeners added through code. The HUD script listens to these events to update the weapon display.
        public event Action OnAmmoChange = null;
        public event Action OnWeaponChange = null;
        public event EventHandler<StateChangedEventArgs> OnStateChange;
        public event OnAlert OnAlertEvent = null;
        public delegate IEnumerator OnAlert(string message, float duration);

        // Components
        private Animator _weaponHolderAnimator = null;
        //private CameraController _cameraController = null;

        // Properties
        private int _currentWeaponIndex = 0;
        private WeaponState _currentState = WeaponState.Idle;
        private float _aimingPercentage = 0f;
        private float _baseFieldOfView = 0f;

        private void Awake()
        {
            //_cameraController = GetComponent<CameraController>();
            _weaponHolderAnimator = weaponHolder.GetComponent<Animator>();
        }

        private void Start()
        {
            // This is only to remind you that if this weapon script is attached to a player, it also needs weapon input.
            if (GetComponent<WeaponInput>() == null)
            {
                Debug.LogError("Weapon component does not work on its own and may require WeaponInput if used for the player.");
            }

            //_baseFieldOfView = _cameraController.currentCamera.fieldOfView;

            // Spawn the weapon model
            UpdateWeaponModel();
        }

        private void Update()
        {
            float zoomed = _baseFieldOfView / CurrentWeapon.WeaponDefinition.AimZoomMultiplier;
            //_cameraController.currentCamera.fieldOfView = Mathf.Lerp(_baseFieldOfView, zoomed, _aimingPercentage);
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
                UpdateWeaponModel();

                // Update listeners
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
                StartCoroutine(SwitchWeapon((_currentWeaponIndex + 1) % heldWeapons.Length));
            }
            else
            {
                // No more space in inventory, replace current weapon with new one
                WeaponItem old = ReplaceWeapon(_currentWeaponIndex, newWeapon);
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
            UpdateWeaponModel();

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
            if (CurrentState != WeaponState.Idle)
            {
                yield break;
            }

            // Must have ammo in the magazine to fire
            if (CurrentWeapon.Magazine > 0)
            {
                // Firing burst type weapon
                if (CurrentWeapon.WeaponDefinition.TriggerType == TriggerType.Burst)
                {
                    // Only fire enough rounds provided sufficient magazine
                    int burst = 3;
                    for (int i = 0; i < burst && CurrentWeapon.Magazine > 0; i++)
                    {
                        // Fire the weapon
                        CurrentState = WeaponState.Firing;
                        Fire();

                        // Show muzzle flash for split second
                        StartCoroutine(FlashMuzzle());

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
                    CurrentState = WeaponState.Firing;
                    Fire();

                    // Show muzzle flash for split second
                    StartCoroutine(FlashMuzzle());

                    yield return new WaitForSeconds(CurrentWeapon.WeaponDefinition.FireRate);
                }
            }

            CurrentState = WeaponState.Idle;

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
            }
        }

        /// <summary>
        /// Reloads currently equipped weapon.
        /// </summary>
        /// <returns>Reload state</returns>
        public IEnumerator ReloadWeapon()
        {
            // Already reloading or not in idle state
            if (CurrentState == WeaponState.Reloading || CurrentState != WeaponState.Idle)
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
            CurrentState = WeaponState.Reloading;

            // Play animation
            // ReloadSpeed is a parameter in the animator. It's the speed multiplier.
            // The reload animation is 1 second total so we multiply the speed of the animation by 1 / ReloadTime
            _weaponHolderAnimator.SetTrigger("Reload");
            _weaponHolderAnimator.SetFloat("ReloadSpeed", 1.0f / CurrentWeapon.WeaponDefinition.ReloadTime);

            yield return new WaitForSeconds(CurrentWeapon.WeaponDefinition.ReloadTime);

            // Fill up magazine with ammo from reserves
            CurrentWeapon.ReloadMagazine();

            // Update listeners
            OnAmmoChange?.Invoke();
            onReload?.Invoke();

            CurrentState = WeaponState.Idle;
        }

        /// <summary>
        /// Switch current weapon to another held weapon by index.
        /// </summary>
        /// <param name="index">Index of weapon to switch to</param>
        /// <returns>Switching state</returns>
        public IEnumerator SwitchWeapon(int index)
        {
            // Cannot switch weapon when not in idle state or current weapon already out
            if (CurrentState != WeaponState.Idle || _currentWeaponIndex == index)
            {
                yield break;
            }

            // Begin switching weapons
            CurrentState = WeaponState.Switching;
            Debug.Log("Switching weapon");

            // Holster animation
            _weaponHolderAnimator.SetTrigger("Holster");
            _weaponHolderAnimator.SetFloat("HolsterSpeed", 1.0f / CurrentWeapon.WeaponDefinition.HolsterTime);

            yield return new WaitForSeconds(CurrentWeapon.WeaponDefinition.HolsterTime);

            // Change the weapon
            _currentWeaponIndex = index;
            UpdateWeaponModel();

            // Update listeners
            OnAmmoChange?.Invoke();
            OnWeaponChange?.Invoke();
            onSwitch?.Invoke();

            // Ready animation
            _weaponHolderAnimator.SetTrigger("Ready");
            _weaponHolderAnimator.SetFloat("ReadySpeed", 1.0f / CurrentWeapon.WeaponDefinition.ReadyTime);

            yield return new WaitForSeconds(CurrentWeapon.WeaponDefinition.ReadyTime);
            Debug.Log("Weapon switch done");
            CurrentState = WeaponState.Idle;
        }

        public void IncreaseAim()
        {
            if (_aimingPercentage >= 1.0f)
            {
                return;
            }

            _aimingPercentage = Mathf.Min(1.0f, _aimingPercentage + 1 / CurrentWeapon.WeaponDefinition.AimTime * Time.deltaTime);
        }

        public void DecreaseAim()
        {
            if (_aimingPercentage <= 0f)
            {
                return;
            }

            _aimingPercentage = Mathf.Max(0f, _aimingPercentage - 1 / CurrentWeapon.WeaponDefinition.AimTime * Time.deltaTime);
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
                    //Transform cameraTransform = _cameraController.currentCamera.transform;
                    // Create ray
                    Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
                    // Raycast using LayerMask
                    bool raycast = Physics.Raycast(ray, out var hit, raycastRange, raycastMask);
                    // Determine objects hit
                    if (raycast)
                    {
                        Debug.Log(CurrentWeapon.WeaponDefinition.WeaponName + " hit target " + hit.transform.name);
                        Debug.DrawLine(muzzle.position, hit.point, Color.red, 0.5f);
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

            // Fire animation
            _weaponHolderAnimator.SetTrigger("Fire");
            _weaponHolderAnimator.SetFloat("FireRate", 1.0f / CurrentWeapon.WeaponDefinition.FireRate);
        }

        /// <summary>
        /// Shows the muzzle flash for a split second and then hides it. The muzzle flash receives a random scale.
        /// </summary>
        /// <returns></returns>
        private IEnumerator FlashMuzzle()
        {
            // Set random scale for muzzle flash object
            Vector3 randomScale = new Vector3(Random.Range(0.5f, 0.8f),Random.Range(0.5f, 0.8f),Random.Range(0.5f, 0.8f));
            muzzleFlash.localScale = randomScale;

            // Show muzzle flash
            muzzleFlash.gameObject.SetActive(true);

            // Hide muzzle flash after split second
            yield return new WaitForSeconds(0.01f);
            muzzleFlash.gameObject.SetActive(false);
        }

        /// <summary>
        /// Removes old weapon model and spawns a new weapon model from the currently equipped weapon.
        /// This process destroys all child game objects from the weapon holder and instantiates a new object
        /// from the model prefab in the weapon definition.
        /// </summary>
        private void UpdateWeaponModel()
        {
            // Reset muzzle transform
            muzzle = null;
            muzzleFlash = null;

            if (CurrentWeapon.WeaponDefinition != null && CurrentWeapon.WeaponDefinition.ModelPrefab != null)
            {
                // Destroy all children
                foreach (Transform child in weaponHolder) {
                    Destroy(child.gameObject);
                }

                // Spawn weapon model
                GameObject weaponModel = Instantiate(CurrentWeapon.WeaponDefinition.ModelPrefab, weaponHolder);
                // Set muzzle transform. The child object must be called Muzzle
                muzzle = weaponModel.transform.Find("Muzzle");
                muzzleFlash = muzzle.transform.GetChild(0);

                if (!isLocalPlayer)
                {
                    weaponModel.SetActive(false);
                }
            }
        }

        public class StateChangedEventArgs : EventArgs
        {
            public WeaponState State { get; set; }
        }
    }
}
