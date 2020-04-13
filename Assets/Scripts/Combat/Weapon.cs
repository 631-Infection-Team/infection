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
        [SerializeField, Tooltip("Percentage reduction when not aiming, based on 45 degree cone spread from camera")]
        private float accuracyReduction = 0.2f;

        [Header("Graphics")]
        [SerializeField] private GameObject bulletImpactVfx = null;
        [SerializeField] private GameObject bulletTrailVfx = null;

        [Header("Transforms for weapon model")]
        [SerializeField] private Transform weaponHolder = null;
        [SerializeField] private Transform muzzle = null;
        [SerializeField] private Transform muzzleFlash = null;
        [SerializeField, Tooltip("Used for rendering on remote players")]
        private Transform rightHand = null;

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
        private Animator _playerAnimator = null;
        private Camera _camera = null;

        // Properties
        private int _currentWeaponIndex = 0;
        private WeaponState _currentState = WeaponState.Idle;
        private float _aimingPercentage = 0f;
        private float _instabilityPercentage;
        private float _baseFieldOfView = 0f;
        private bool _reloadInterrupt = false;

        private void Awake()
        {
            raycastMask = LayerMask.GetMask("Default");
            _camera = GetComponent<Player>().cam;
            _playerAnimator = GetComponent<Player>().animator;
            _weaponHolderAnimator = weaponHolder.GetComponent<Animator>();
        }

        private IEnumerator Start()
        {
            // This is only to remind you that if this weapon script is attached to a player, it also needs weapon input.
            if (GetComponent<WeaponInput>() == null)
            {
                Debug.LogError("Weapon component does not work on its own and may require WeaponInput if used for the player.");
            }

            // Fill up all held weapons to max ammo
            foreach (WeaponItem weaponItem in heldWeapons)
            {
                weaponItem.FillUpAmmo();
            }
            OnAmmoChange?.Invoke();

            // Store starting field of view to unzoom the camera when transitioning from aiming to not aiming
            _baseFieldOfView = _camera.fieldOfView;

            // Spawn the weapon model and play ready weapon animation
            UpdateAnimatorOverride();
            yield return new WaitUntil(() => _weaponHolderAnimator.isActiveAndEnabled);
            StartCoroutine(ReadyAnimation());
            OnWeaponChange?.Invoke();
            UpdateWeaponModel();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            UpdateRemoteWeaponModel();
        }

        private void Update()
        {
            if (!isLocalPlayer)
            {
                return;
            }

            // Zoom in based on aiming percentage
            float zoomed = _baseFieldOfView / CurrentWeapon.WeaponDefinition.AimZoomMultiplier;
            _camera.fieldOfView = Mathf.Lerp(_baseFieldOfView, zoomed, _aimingPercentage);

            // Gradually reduce instability percentage while weapon is calming down
            if (InstabilityPercentage > 0f && CurrentState != WeaponState.Firing)
            {
                InstabilityPercentage = Mathf.Max(0f, InstabilityPercentage - Time.deltaTime);
            }
        }

        private void OnEnable()
        {
            OnWeaponChange += UpdateAnimatorWeaponType;
        }

        private void OnDisable()
        {
            OnWeaponChange -= UpdateAnimatorWeaponType;
        }

        /// <summary>
        /// Equip a new weapon in an empty slot. If there are no empty slots, replace currently equipped weapon.
        /// </summary>
        /// <param name="newWeapon">New weapon to equip</param>
        public WeaponItem EquipWeapon(WeaponItem newWeapon)
        {
            // Player has no weapons
            // TODO: Find solution for edge case where player has other weapons but current weapon is null
            if (HasMoreWeapons && CurrentWeapon == null)
            {
            }

            WeaponItem oldWeapon = null;
            if (CurrentWeapon == null)
            {
                CurrentWeapon = newWeapon;
                UpdateWeaponModel();
                UpdateAnimatorOverride();
            }
            else
            {
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
                    oldWeapon = ReplaceWeapon(_currentWeaponIndex, newWeapon);
                    Debug.Log("Replaced " + oldWeapon.WeaponDefinition.WeaponName + " with " + newWeapon.WeaponDefinition.WeaponName);
                }
            }

            // Update listeners
            OnWeaponChange?.Invoke();
            OnAmmoChange?.Invoke();
            onEquip?.Invoke();

            return oldWeapon;
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
            UpdateAnimatorOverride();

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
            if (!isLocalPlayer)
            {
                yield break;
            }

            // Cannot fire weapon when state is not idle or when reload action interrupts fire
            if (CurrentState != WeaponState.Idle || _reloadInterrupt)
            {
                yield break;
            }

            // Must have ammo in the magazine to fire
            if (CurrentWeapon.Magazine > 0)
            {
                // Firing burst type weapon
                if (CurrentWeapon.WeaponDefinition.TriggerType == TriggerType.Burst)
                {
                    bool animationStarted = false;
                    // Only fire enough rounds provided sufficient magazine
                    int burst = 3;
                    for (int i = 0; i < burst && CurrentWeapon.Magazine > 0; i++)
                    {
                        // Fire the weapon
                        CurrentState = WeaponState.Firing;
                        _playerAnimator.SetTrigger("Shoot_t");
                        CmdFire();

                        // Play fire animation once per burst
                        if (!animationStarted)
                        {
                            _weaponHolderAnimator.SetTrigger("Fire");
                            animationStarted = true;
                            // Animation plays 3 times faster for burst weapons
                            _weaponHolderAnimator.SetFloat("FireRate", 1.0f / CurrentWeapon.WeaponDefinition.FireRate / 3f);
                        }

                        // Wait a third of the fire rate between each shot in the burst
                        yield return new WaitForSeconds(CurrentWeapon.WeaponDefinition.FireRate / 3.0f);
                    }

                    // Wait three times as long between bursts
                    yield return new WaitForSeconds(CurrentWeapon.WeaponDefinition.FireRate * 3.0f);
                }
                else
                {
                    // Firing automatic or manual type weapon
                    // Fire the weapon
                    CurrentState = WeaponState.Firing;
                    _playerAnimator.SetTrigger("Shoot_t");
                    CmdFire();

                    // Fire animation
                    _weaponHolderAnimator.SetTrigger("Fire");
                    _weaponHolderAnimator.SetFloat("FireRate", 1.0f / CurrentWeapon.WeaponDefinition.FireRate);

                    yield return new WaitForSeconds(CurrentWeapon.WeaponDefinition.FireRate);
                }
            }

            CurrentState = WeaponState.Idle;

            // Out of ammo
            if (CurrentWeapon.Magazine <= 0)
            {
                if (CurrentWeapon.Reserves == 0)
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
            // Cannot reload weapon if dead
            if (Player.localPlayer.health <= 0)
            {
                yield break;
            }

            // Already reloading or not in idle state
            if (CurrentState == WeaponState.Reloading)
            {
                yield break;
            }

            _reloadInterrupt = true;
            yield return new WaitUntil(() => CurrentState == WeaponState.Idle);

            // No more ammo
            if (CurrentWeapon.Reserves == 0)
            {
                Debug.Log("No more ammo in reserves!");
                StartCoroutine(OnAlertEvent?.Invoke("Out of ammo", 1f));
                _reloadInterrupt = false;
                yield break;
            }

            // Weapon already fully reloaded
            if (CurrentWeapon.Magazine >= CurrentWeapon.WeaponDefinition.ClipSize)
            {
                Debug.Log("Magazine fully loaded, no need to reload.");
                StartCoroutine(OnAlertEvent?.Invoke("Magazine full", 1f));
                _reloadInterrupt = false;
                yield break;
            }

            // Start reloading
            CurrentState = WeaponState.Reloading;
            _reloadInterrupt = false;

            // Play animation
            // ReloadSpeed is a parameter in the animator. It's the speed multiplier.
            // The reload animation is 1 second total so we multiply the speed of the animation by 1 / ReloadTime
            _weaponHolderAnimator.SetTrigger("Reload");
            _weaponHolderAnimator.SetFloat("ReloadSpeed", 1.0f / CurrentWeapon.WeaponDefinition.ReloadTime);
            _playerAnimator.SetTrigger("Reload_t");

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
            // Cannot switch weapon if dead
            if (Player.localPlayer.health <= 0)
            {
                yield break;
            }

            // Cannot switch weapon when not in idle state or current weapon already out
            if (CurrentState != WeaponState.Idle || _currentWeaponIndex == index)
            {
                yield break;
            }

            // Begin switching weapons
            CurrentState = WeaponState.Switching;

            // Play holster animation
            yield return StartCoroutine(HolsterAnimation());

            // Change the weapon
            _currentWeaponIndex = index;
            UpdateWeaponModel();
            UpdateAnimatorOverride();

            // Update listeners
            OnAmmoChange?.Invoke();
            OnWeaponChange?.Invoke();
            onSwitch?.Invoke();

            // Play ready animation
            yield return StartCoroutine(ReadyAnimation());

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
        /// Set the aim percentage based on axis input using lerp for smooth transition. This supports analog axis input.
        /// </summary>
        /// <param name="axis"></param>
        public void SetAim(float axis)
        {
            // Unzoom while reloading or switching, otherwise zoom normally
            float aim = CurrentState == WeaponState.Reloading || CurrentState == WeaponState.Switching ? 0f : axis;
            AimingPercentage = Mathf.Clamp01(Mathf.MoveTowards(AimingPercentage, aim, 1f / CurrentWeapon.WeaponDefinition.AimTime * Time.deltaTime));

            if (_weaponHolderAnimator.isActiveAndEnabled)
            {
                // Update animator to show aiming transition
                _weaponHolderAnimator.SetFloat("AimPercentage", AimingPercentage);
            }
        }

        /// <summary>
        /// Fire the weapon. Waiting for weapon state is not handled here.
        /// This method is only used to raycast and consume ammo.
        /// </summary>

        [Command]
        public void CmdFire()
        {
            if (!Player.localPlayer.canShoot) return;

            Vector3 influence = CalculateAccuracyInfluence();

            switch (CurrentWeapon.WeaponDefinition.WeaponType)
            {
                case WeaponType.Raycast:
                    // Create ray with accuracy influence
                    Ray ray = GenerateRay(influence);

                    // Raycast using LayerMask
                    bool raycast = Physics.Raycast(ray, out var hit, raycastRange, raycastMask);

                    // Determine objects hit
                    if (raycast)
                    {
                        Player targetPlayer = hit.transform.gameObject.GetComponent<Player>();

                        if (targetPlayer)
                        {
                            Player.localPlayer.DealDamageTo(targetPlayer, CurrentWeapon.WeaponDefinition.Damage);
                            GameObject projectile = Instantiate(targetPlayer.bloodImpactVfx, hit.point, Quaternion.LookRotation(Vector3.Reflect(ray.direction, hit.normal)));
                            NetworkServer.Spawn(projectile);
                        }
                        else
                        {
                            GameObject projectile = Instantiate(bulletImpactVfx, hit.point, Quaternion.LookRotation(Vector3.Reflect(ray.direction, hit.normal)));
                            NetworkServer.Spawn(projectile);
                        }

                        Debug.Log(CurrentWeapon.WeaponDefinition.WeaponName + " hit target " + hit.transform.name);
                        // Debug.DrawLine(muzzle.position, hit.point, Color.red, 0.5f);
                    }

                    // Create bullet trail regardless if raycast hit and quickly destroy it if it does not collide
                    GameObject trail = Instantiate(bulletTrailVfx, muzzle.position, Quaternion.LookRotation(ray.direction));
                    LineRenderer lineRenderer = trail.GetComponent<LineRenderer>();
                    lineRenderer.SetPosition(0, muzzle.position);

                    if (raycast)
                    {
                        lineRenderer.SetPosition(1, hit.point);
                    }
                    else
                    {
                        lineRenderer.SetPosition(1, ray.direction * 10000f);
                    }

                    Destroy(trail, Time.deltaTime);
                    break;

                case WeaponType.Projectile:
                    // TODO: Implement projectile weapon firing
                    break;
            }

            if (!CurrentWeapon.WeaponDefinition.Silencer)
            {
                // Show muzzle flash for split second
                StartCoroutine(FlashMuzzle(influence));
            }

            // Apply recoil
            InstabilityPercentage = Mathf.Min(1f, InstabilityPercentage + CurrentWeapon.WeaponDefinition.RecoilMultiplier);

            // Subtract ammo
            CurrentWeapon.ConsumeMagazine(1);

            // Update listeners
            OnAmmoChange?.Invoke();
            onFire?.Invoke();
        }

        [ClientRpc]
        public void RpcOnFire()
        {
            if (!Player.localPlayer.canShoot) return;
        }

        /// <summary>
        /// Generate Vector3 influence using weapon accuracy which will sway the bullet trajectory.
        /// </summary>
        /// <returns></returns>
        private Vector3 CalculateAccuracyInfluence()
        {
            // Cache variables for repeated access
            Transform cameraTransform = _camera.transform;

            // Accuracy reduction when not aiming
            float reduction = CurrentWeapon.WeaponDefinition.Accuracy * accuracyReduction * (1f - AimingPercentage);
            float accuracy = CurrentWeapon.WeaponDefinition.Accuracy - reduction;

            // Divide by 4 to reduce the max angle from 180 degrees to 45 degrees
            return (cameraTransform.right * Random.Range(-1f + accuracy, 1f - accuracy) + cameraTransform.up * Random.Range(-1f + accuracy, 1f - accuracy)) / 4f;
        }

        /// <summary>
        /// Create a ray from camera transform using an influence vector created from accuracy to rotate ray.
        /// </summary>
        /// <param name="influence"></param>
        /// <returns>Accuracy influenced ray</returns>
        private Ray GenerateRay(Vector3 influence)
        {
            // Cache camera transform for repeated access
            Transform cameraTransform = _camera.transform;
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
            Vector3 randomScale = new Vector3(Random.Range(0.3f, 1f), Random.Range(0.3f, 1f), Random.Range(0.3f, 1f));
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
                foreach (Transform child in weaponHolder)
                {
                    Destroy(child.gameObject);
                }

                // Spawn weapon model
                GameObject weaponModel = Instantiate(CurrentWeapon.WeaponDefinition.ModelPrefab, weaponHolder);
                // Set muzzle transform. The child object must be called Muzzle
                muzzle = weaponModel.transform.Find("Muzzle");
                muzzleFlash = muzzle.transform.GetChild(0);

                weaponModel.SetActive(isLocalPlayer);
            }
        }

        [Server]
        private void UpdateRemoteWeaponModel()
        {
            if (CurrentWeapon.WeaponDefinition != null && CurrentWeapon.WeaponDefinition.RemoteModelPrefab != null)
            {
                // Destroy all children
                foreach (Transform child in rightHand)
                {
                    Destroy(child.gameObject);
                }

                // Spawn weapon model to show other players
                GameObject remoteModel = Instantiate(CurrentWeapon.WeaponDefinition.RemoteModelPrefab, rightHand);

                // Other players can see this weapon model but the local player cannot
                remoteModel.SetActive(!isLocalPlayer);
            }
        }

        private void UpdateAnimatorWeaponType()
        {
            _playerAnimator.SetInteger("WeaponType_int", CurrentWeapon.WeaponDefinition.WeaponClass.AnimatorType);
            _playerAnimator.SetBool("FullAuto_b", CurrentWeapon.WeaponDefinition.TriggerType == TriggerType.Auto);
        }

        private void UpdateAnimatorOverride()
        {
            // Update animator override or reset to default animator controller
            var overrideController = _weaponHolderAnimator.runtimeAnimatorController as AnimatorOverrideController;
            if (CurrentWeapon.WeaponDefinition.AnimatorOverride != null)
            {
                _weaponHolderAnimator.runtimeAnimatorController = CurrentWeapon.WeaponDefinition.AnimatorOverride;
            }
            else if (overrideController != null)
            {
                _weaponHolderAnimator.runtimeAnimatorController = overrideController.runtimeAnimatorController;
            }
        }

        /// <summary>
        /// Play the weapon holster animation for the duration of the holster time defined in the weapon definition.
        /// </summary>
        /// <returns>Holster animation</returns>
        private IEnumerator HolsterAnimation()
        {
            // Start playing animation for holster time
            _weaponHolderAnimator.SetFloat("HolsterSpeed", 1.0f / CurrentWeapon.WeaponDefinition.HolsterTime);
            _weaponHolderAnimator.SetBool("Holster", true);

            // Wait for animation to finish
            yield return new WaitForSeconds(CurrentWeapon.WeaponDefinition.HolsterTime);

            // Reset animator parameters
            _weaponHolderAnimator.SetBool("Holster", false);
            _weaponHolderAnimator.SetFloat("HolsterSpeed", 0f);
        }

        /// <summary>
        /// Play the weapon ready animation for the duration of the ready time defined in the weapon definition.
        /// </summary>
        /// <returns>Ready animation</returns>
        private IEnumerator ReadyAnimation()
        {
            CurrentState = WeaponState.Switching;
            // Start playing animation for ready time
            _weaponHolderAnimator.SetFloat("ReadySpeed", 1.0f / CurrentWeapon.WeaponDefinition.ReadyTime);
            _weaponHolderAnimator.SetBool("Ready", true);

            // Wait for animation to finish
            yield return new WaitForSeconds(CurrentWeapon.WeaponDefinition.ReadyTime);

            // Reset animator parameters
            _weaponHolderAnimator.SetBool("Ready", false);
            _weaponHolderAnimator.SetFloat("ReadySpeed", 0f);
            CurrentState = WeaponState.Idle;
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
