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
        [SerializeField] private GameObject bulletImpactVfx = null;

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
        /// The percentage that represents the state of how fully complete the aiming down the sights action is.
        /// A value of 0 means not aiming down the sights at all. A value of 1 means fully aiming down the sights.
        /// Any value in between means currently transitioning from either fully aiming or not aiming.
        /// </summary>
        public float AimingPercentage
        {
            get => _aimingPercentage;
            set
            {
                _aimingPercentage = value;
                OnAimingChange?.Invoke(this, new PercentageEventArgs
                {
                    Percentage = _aimingPercentage
                });
            }
        }

        public float InstabilityPercentage
        {
            get => _instabilityPercentage;
            set
            {
                _instabilityPercentage = value;
                OnRecoil?.Invoke(this, new PercentageEventArgs
                {
                    Percentage = _instabilityPercentage
                });
            }
        }

        /// <summary>
        /// The player cannot hold additional weapons as their held weapons array is full.
        /// </summary>
        public bool IsFullOfWeapons => !Array.Exists(heldWeapons, w => w == null);

        /// <summary>
        /// The player has other weapons that is not the currently equipped weapon.
        /// </summary>
        public bool HasMoreWeapons => Array.Exists(heldWeapons, w => w != null && w != CurrentWeapon && CurrentWeapon != null);

        // Events. Listeners added through code. The HUD script listens to these events to update the weapon display.
        public event Action OnAmmoChange = null;
        public event Action OnWeaponChange = null;
        public event EventHandler<StateChangedEventArgs> OnStateChange;
        public event EventHandler<PercentageEventArgs> OnAimingChange;
        public event EventHandler<PercentageEventArgs> OnRecoil;
        public event OnAlert OnAlertEvent = null;
        public delegate IEnumerator OnAlert(string message, float duration);

        // Components
        private Animator _weaponHolderAnimator = null;
        private RuntimeAnimatorController _defaultWeaponAnimator = null;
        private CameraController _cameraController = null;

        // Properties
        private int _currentWeaponIndex = 0;
        private WeaponState _currentState = WeaponState.Idle;
        private float _aimingPercentage = 0f;
        private float _instabilityPercentage;
        private float _baseFieldOfView = 0f;

        private void Awake()
        {
            _cameraController = GetComponent<CameraController>();
            _weaponHolderAnimator = weaponHolder.GetComponent<Animator>();

            // Cache the default animator in case animator overrides become null when switching weapons
            _defaultWeaponAnimator = _weaponHolderAnimator.runtimeAnimatorController;
        }

        private void Start()
        {
            // This is only to remind you that if this weapon script is attached to a player, it also needs weapon input.
            if (GetComponent<WeaponInput>() == null)
            {
                Debug.LogError("Weapon component does not work on its own and may require WeaponInput if used for the player.");
            }

            // Store starting field of view to unzoom the camera when transitioning from aiming to not aiming
            _baseFieldOfView = _cameraController.currentCamera.fieldOfView;

            // Spawn the weapon model
            UpdateWeaponModel();
        }

        private void Update()
        {
            // Zoom in based on aiming percentage
            float zoomed = _baseFieldOfView / CurrentWeapon.WeaponDefinition.AimZoomMultiplier;
            _cameraController.currentCamera.fieldOfView = Mathf.Lerp(_baseFieldOfView, zoomed, AimingPercentage);

            // Gradually reduce instability percentage while weapon is calming down
            if (InstabilityPercentage > 0f && CurrentState != WeaponState.Firing)
            {
                InstabilityPercentage = Mathf.Max(0f, InstabilityPercentage - Time.deltaTime);
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
            if (HasMoreWeapons && CurrentWeapon == null)
            {

            }

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
                CycleWeapons();
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

        /// <summary>
        /// Switch to the next weapon by incrementing index. If index is out of bounds, wrap to other end of array.
        /// This operation works both ways.
        /// </summary>
        public void CycleWeapons(int direction = 1)
        {
            int index = (((_currentWeaponIndex + direction) % heldWeapons.Length) + heldWeapons.Length) % heldWeapons.Length;
            StartCoroutine(SwitchWeapon(index));
        }

        /// <summary>
        /// Increase the aiming percentage based on current weapon's aim time.
        /// </summary>
        public void IncreaseAim()
        {
            // Unzoom while reloading or switching
            if (CurrentState == WeaponState.Reloading || CurrentState == WeaponState.Switching)
            {
                DecreaseAim();
                return;
            }

            if (AimingPercentage >= 1.0f)
            {
                return;
            }

            // Upper bound is 1
            float value = Mathf.Min(1.0f, AimingPercentage + 1 / CurrentWeapon.WeaponDefinition.AimTime * Time.deltaTime);
            AimingPercentage = value;

            // Update animator to show aiming transition
            _weaponHolderAnimator.SetBool("Aim", true);
            _weaponHolderAnimator.SetFloat("AimTime", AimingPercentage);
        }

        /// <summary>
        /// Decrease the aiming percentage based on current weapon's aim time.
        /// </summary>
        public void DecreaseAim()
        {
            if (AimingPercentage <= 0f)
            {
                return;
            }

            // Lower bound is 0
            float value = Mathf.Max(0f, AimingPercentage - 1 / CurrentWeapon.WeaponDefinition.AimTime * Time.deltaTime);
            AimingPercentage = value;

            // Update animator to show aiming transition
            _weaponHolderAnimator.SetBool("Aim", false);
            _weaponHolderAnimator.SetFloat("AimTime", AimingPercentage);
        }

        /// <summary>
        /// Fire the weapon. Waiting for weapon state is not handled here.
        /// This method is only used to raycast and consume ammo.
        /// </summary>
        private void Fire()
        {
            // Store variables for repeated access
            Transform cameraTransform = _cameraController.currentCamera.transform;
            float accuracy = CurrentWeapon.WeaponDefinition.Accuracy;
            // Generate influence using weapon accuracy
            Vector3 influence = cameraTransform.right * Random.Range(-1f + accuracy, 1f - accuracy) +
                                cameraTransform.up * Random.Range(-1f + accuracy, 1f - accuracy);

            switch (CurrentWeapon.WeaponDefinition.WeaponType)
            {
                case WeaponType.Raycast:
                    // Create ray with accuracy influence
                    Ray ray = GenerateRay(influence);

                    // Raycast using LayerMask
                    bool raycast = Physics.Raycast(ray, out var hit, raycastRange, raycastMask);

                    // Determine objects hit
                    if (_cameraController && raycast)
                    {
                        // Generate bullet impact effects. Particle system automatically destroys the object when finished.
                        Instantiate(bulletImpactVfx, hit.point, Quaternion.LookRotation(Vector3.Reflect(ray.direction, hit.normal)));

                        Debug.Log(CurrentWeapon.WeaponDefinition.WeaponName + " hit target " + hit.transform.name);
                        Debug.DrawLine(muzzle.position, hit.point, Color.red, 0.5f);
                    }
                    break;

                case WeaponType.Projectile:
                    // TODO: Implement projectile weapon firing
                    break;
            }

            // Show muzzle flash for split second
            StartCoroutine(FlashMuzzle(influence));

            // Apply recoil
            InstabilityPercentage = Mathf.Min(1f, InstabilityPercentage + CurrentWeapon.WeaponDefinition.RecoilMultiplier);

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
        /// Create a ray from camera transform using an influence vector created from accuracy to rotate ray.
        /// </summary>
        /// <param name="influence"></param>
        /// <returns>Accuracy influenced ray</returns>
        private Ray GenerateRay(Vector3 influence)
        {
            // Cache camera transform
            Transform cameraTransform = _cameraController.currentCamera.transform;

            // Generate direction with slight random rotation using accuracy
            Vector3 direction = cameraTransform.forward + influence;

            // Create ray using camera position and direction
            return new Ray(cameraTransform.position, direction);
        }

        /// <summary>
        /// Shows the muzzle flash for a split second and then hides it. The muzzle flash receives a random scale.
        /// </summary>
        /// <param name="influence">The amount of rotation applied based on weapon accuracy</param>
        /// <returns>Muzzle flash effect</returns>
        private IEnumerator FlashMuzzle(Vector3 influence)
        {
            // Set random scale and rotation for muzzle flash object
            Vector3 randomScale = new Vector3(Random.Range(0.3f, 1f),Random.Range(0.3f, 1f),Random.Range(0.3f, 1f));
            Vector3 randomRotation = new Vector3(0f, 0f, Random.Range(0f, 360f)) + influence;
            muzzleFlash.localScale = randomScale;
            muzzleFlash.localRotation = Quaternion.Euler(randomRotation);

            // Show muzzle flash
            muzzleFlash.gameObject.SetActive(true);

            // Hide muzzle flash after split second
            yield return new WaitForSeconds(Time.deltaTime);
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

                // Update animator override
                if (CurrentWeapon.WeaponDefinition.AnimatorOverride)
                {
                    _weaponHolderAnimator.runtimeAnimatorController = CurrentWeapon.WeaponDefinition.AnimatorOverride;
                }
                else
                {
                    _weaponHolderAnimator.runtimeAnimatorController = _defaultWeaponAnimator;
                }

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

        public class PercentageEventArgs : EventArgs
        {
            public float Percentage { get; set; }
        }
    }
}
