using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Mirror;
using Random = UnityEngine.Random;

namespace Infection.Combat
{
    [RequireComponent(typeof(Player))]
    [RequireComponent(typeof(PlayerCamera))]
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
        public WeaponItem[] startingWeapons = new WeaponItem[2];
        public SyncListWeaponItem heldWeapons = new SyncListWeaponItem();
        public float raycastRange = 1000f;
        public LayerMask raycastMask = 0;
        [Tooltip("Percentage reduction when not aiming, based on 45 degree cone spread from camera")]
        public float accuracyReduction = 0.2f;

        [Header("Graphics")]
        public GameObject bulletImpactVfx = null;
        public GameObject bulletTrailVfx = null;

        [Header("Transforms for weapon model")]
        public Transform weaponHolder = null;
        public Transform muzzle = null;
        public Transform remoteMuzzle = null;
        public Transform muzzleFlash = null;
        [Tooltip("Used for rendering on remote players")]
        public Transform rightHand = null;

        /// <summary>
        /// Current state of the weapon. This can be idle, firing, reloading, or switching.
        /// </summary>
        public WeaponState CurrentState
        {
            get => _currentState;
            set
            {
                _currentState = value;
                EventOnStateChange?.Invoke(_currentState);
            }
        }

        /// <summary>
        /// The weapon currently in use. Returns one weapon item from the player's held weapons.
        /// </summary>
        public WeaponItem CurrentWeapon
        {
            get
            {
                if (heldWeapons.Count > 0)
                {
                    return heldWeapons[_currentWeaponIndex];
                }
                else
                {
                    return null;
                }

            }

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
                EventOnAimingChange?.Invoke(_aimingPercentage);
            }
        }

        public float InstabilityPercentage
        {
            get => _instabilityPercentage;
            set
            {
                _instabilityPercentage = value;
                EventOnRecoil?.Invoke(_instabilityPercentage);
            }
        }

        /// <summary>
        /// The player cannot hold additional weapons as their held weapons array is full.
        /// </summary>
        public bool IsFullOfWeapons => heldWeapons.All(w => w != null);

        /// <summary>
        /// The player has other weapons that is not the currently equipped weapon.
        /// </summary>
        public bool HasMoreWeapons => heldWeapons.Any(w => w != null && w != CurrentWeapon && w.weaponDefinition != null && (CurrentWeapon != null || CurrentWeapon.weaponDefinition != null));

        #region Events
        // Events. Listeners added through code. The HUD script listens to these events to update the weapon display.
        [SyncEvent] public event Action EventOnAmmoChange = null;
        [SyncEvent] public event Action EventOnWeaponChange = null;
        [SyncEvent] public event StateChangedEvent EventOnStateChange;
        [SyncEvent] public event PercentageEvent EventOnAimingChange;
        [SyncEvent] public event PercentageEvent EventOnRecoil;
        [SyncEvent] public event OnAlert EventOnAlert = null;
        public delegate IEnumerator OnAlert(string message, float duration);
        public delegate void StateChangedEvent(WeaponState state);
        public delegate void PercentageEvent(float percentage);
        #endregion

        #region Private Fields
        // Components
        private Animator _weaponHolderAnimator = null;
        private Camera _camera = null;

        // Private fields
        private int _currentWeaponIndex = 0;
        private WeaponState _currentState = WeaponState.Idle;
        private float _aimingPercentage = 0f;
        private float _instabilityPercentage;
        private float _baseFieldOfView = 0f;
        private bool _reloadInterrupt = false;
        private bool _fireDown = false;
        #endregion

        private void Awake()
        {
            _camera = GetComponent<Player>().camera;
            _weaponHolderAnimator = weaponHolder.GetComponent<Animator>();
        }

        private IEnumerator Start()
        {
            // Fill up all held weapons to max ammo
            foreach (WeaponItem weaponItem in heldWeapons)
            {
                weaponItem.CmdFillUpAmmo();
            }
            EventOnAmmoChange?.Invoke();

            // Store starting field of view to unzoom the camera when transitioning from aiming to not aiming
            _baseFieldOfView = _camera.fieldOfView;

            // Spawn the weapon model and play ready weapon animation
            UpdateAnimatorOverride();
            yield return new WaitUntil(() => _weaponHolderAnimator.isActiveAndEnabled);
            CmdReadyAnimation();
            EventOnWeaponChange?.Invoke();
            UpdateWeaponModel();
        }

        public void Update()
        {
            if (!isLocalPlayer) return;

            // Reset fire when trigger is released
            if (Input.GetAxis("Fire") <= 0f)
            {
                _fireDown = false;
            }

            if (CurrentWeapon != null && CurrentWeapon.weaponDefinition)
            {
                switch (CurrentWeapon.weaponDefinition.triggerType)
                {
                    case TriggerType.Auto:
                    // Automatic fire is the same as burst
                    case TriggerType.Burst:
                        // Currently you can hold down Fire to fire burst mode weapons
                        if (Input.GetAxis("Fire") > 0f)
                        {
                            StartCoroutine(FireWeapon());
                        }
                        break;

                    case TriggerType.Manual:
                        // Manual fire
                        if (Input.GetAxis("Fire") > 0f)
                        {
                            if (!_fireDown)
                            {
                                _fireDown = true;
                                StartCoroutine(FireWeapon());
                            }
                        }
                        break;
                }

                // Reload weapon
                if (Input.GetButtonDown("Reload"))
                {
                    StartCoroutine(ReloadWeapon());
                }

                // Aiming down the sights, in-between is possible with gamepad trigger
                SetAim(Input.GetAxis("Aim"));
            }

            if (HasMoreWeapons)
            {
                // Scroll up XOR Gamepad Button 3
                if (Input.GetAxis("Mouse ScrollWheel") > 0f ^ Input.GetButtonDown("Switch"))
                {
                    // Switch to the next weapon
                    CycleWeapons();
                }
                // Scroll down
                else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
                {
                    // Switch to the previous weapon
                    CycleWeapons(-1);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    // Switch to first weapon
                    StartCoroutine(SwitchWeapon(0));
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    // Switch to second weapon
                    StartCoroutine(SwitchWeapon(1));
                }
            }
        }

        public void LateUpdate()
        {
            if (CurrentWeapon != null && CurrentWeapon.weaponDefinition != null)
            {
                // Zoom in based on aiming percentage
                float zoomed = _baseFieldOfView / CurrentWeapon.weaponDefinition.aimZoomMultiplier;
                _camera.fieldOfView = Mathf.Lerp(_baseFieldOfView, zoomed, _aimingPercentage);
            }

            // Gradually reduce instability percentage while weapon is calming down
            if (InstabilityPercentage > 0f && CurrentState != WeaponState.Firing)
            {
                InstabilityPercentage = Mathf.Max(0f, InstabilityPercentage - Time.deltaTime);
            }
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            CmdUpdateRemoteWeaponModel();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            heldWeapons.Callback += OnWeaponsUpdated;
            heldWeapons.Add(startingWeapons[0]);
            heldWeapons.Add(startingWeapons[1]);
        }

        private void OnEnable()
        {
            EventOnWeaponChange += CmdUpdateAnimatorWeaponType;
        }

        private void OnDisable()
        {
            EventOnWeaponChange -= CmdUpdateAnimatorWeaponType;
        }

        private void OnWeaponsUpdated(SyncListWeaponItem.Operation op, int index, WeaponItem oldItem, WeaponItem newItem)
        {
            switch (op)
            {
                case SyncListWeaponItem.Operation.OP_ADD:
                    // index is where it got added in the list
                    // item is the new item
                    break;
                case SyncListWeaponItem.Operation.OP_CLEAR:
                    // list got cleared
                    break;
                case SyncListWeaponItem.Operation.OP_INSERT:
                    // index is where it got added in the list
                    // item is the new item
                    break;
                case SyncListWeaponItem.Operation.OP_REMOVEAT:
                    // index is where it got removed in the list
                    // item is the item that was removed
                    break;
                case SyncListWeaponItem.Operation.OP_SET:
                    // index is the index of the item that was updated
                    // item is the previous item
                    break;
            }
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
            if (CurrentWeapon == null || CurrentWeapon.weaponDefinition == null)
            {
                CmdEquipWeapon(newWeapon, _currentWeaponIndex);
                UpdateWeaponModel();
                CmdUpdateRemoteWeaponModel();
                UpdateAnimatorOverride();
            }
            else
            {
                // Player has an empty slot in inventory
                int emptySlot = heldWeapons.FindIndex(w => w == null);
                if (emptySlot > -1)
                {
                    // Equip the new weapon and switch to it
                    CmdEquipWeapon(newWeapon, emptySlot);
                    CycleWeapons();
                }
                else
                {
                    // No more space in inventory, replace current weapon with new one
                    oldWeapon = ReplaceWeapon(_currentWeaponIndex, newWeapon);
                    Debug.Log("Replaced " + oldWeapon.weaponDefinition.weaponName + " with " + newWeapon.weaponDefinition.weaponName);
                }
            }

            // Update listeners
            EventOnWeaponChange?.Invoke();
            EventOnAmmoChange?.Invoke();

            return oldWeapon;
        }

        [Command]
        private void CmdEquipWeapon(WeaponItem newWeapon, int slotIndex)
        {
            heldWeapons[slotIndex] = newWeapon;
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
            CmdUpdateRemoteWeaponModel();
            UpdateAnimatorOverride();

            // Update listeners
            EventOnWeaponChange?.Invoke();

            return oldWeapon;
        }

        public IEnumerator FireWeapon()
        {
            // Cannot fire weapon when state is not idle or when reload action interrupts fire
            if (CurrentState != WeaponState.Idle || _reloadInterrupt)
            {
                yield break;
            }

            // Must have ammo in the magazine to fire
            if (CurrentWeapon.magazine > 0)
            {
                // Firing burst type weapon
                if (CurrentWeapon.weaponDefinition.triggerType == TriggerType.Burst)
                {
                    bool animationStarted = false;
                    // Only fire enough rounds provided sufficient magazine
                    int burst = 3;
                    for (int i = 0; i < burst && CurrentWeapon.magazine > 0; i++)
                    {
                        // Fire the weapon
                        CurrentState = WeaponState.Firing;
                        //playerAnimator.Animator.SetTrigger("Shoot_t");
                        CmdFire();

                        // Play fire animation once per burst
                        if (!animationStarted)
                        {
                            _weaponHolderAnimator.SetTrigger("Fire");
                            animationStarted = true;
                            // Animation plays 3 times faster for burst weapons
                            _weaponHolderAnimator.SetFloat("FireRate", 1.0f / CurrentWeapon.weaponDefinition.fireRate / 3f);
                        }

                        // Wait a third of the fire rate between each shot in the burst
                        yield return new WaitForSeconds(CurrentWeapon.weaponDefinition.fireRate / 3.0f);
                    }

                    // Wait three times as long between bursts
                    yield return new WaitForSeconds(CurrentWeapon.weaponDefinition.fireRate * 3.0f);
                }
                else
                {
                    // Firing automatic or manual type weapon
                    // Fire the weapon
                    CurrentState = WeaponState.Firing;
                    //_playerAnimator.SetTrigger("Shoot_t");
                    CmdFire();

                    // Fire animation
                    _weaponHolderAnimator.SetTrigger("Fire");
                    _weaponHolderAnimator.SetFloat("FireRate", 1.0f / CurrentWeapon.weaponDefinition.fireRate);

                    yield return new WaitForSeconds(CurrentWeapon.weaponDefinition.fireRate);
                }
            }

            CurrentState = WeaponState.Idle;

            // Out of ammo
            if (CurrentWeapon.magazine <= 0)
            {
                if (CurrentWeapon.reserves == 0)
                {
                    Debug.Log("Out of ammo!");
                    StartCoroutine(EventOnAlert?.Invoke("Out of ammo", 2f));
                    // Switch to a different weapon if it exists and if it still has ammo left
                    // int nextWeapon = Array.FindIndex(heldWeapons, w => w != null && w.magazine + w.reserves > 0);
                    int nextWeapon = heldWeapons.FindIndex(w => w != null && w.magazine + w.reserves > 0);
                    if (nextWeapon > -1)
                    {
                        StartCoroutine(SwitchWeapon(nextWeapon));
                    }

                    yield break;
                }

                StartCoroutine(ReloadWeapon());
            }
        }

        [Command]
        void CmdFire()
        {
            Vector3 influence = CalculateAccuracyInfluence();

            switch (CurrentWeapon.weaponDefinition.weaponType)
            {
                case WeaponType.Raycast:
                    // We need to calculate the raycast on the server side, because we cannot send gameobjects over the network.
                    Transform cameraTransform = _camera.transform;
                    // Generate direction with slight random rotation using accuracy
                    Vector3 direction = cameraTransform.forward + influence;
                    // Create ray using camera position and direction
                    Ray ray = new Ray(cameraTransform.position, direction);
                    // Raycast using LayerMask
                    bool raycast = Physics.Raycast(ray, out var hit, raycastRange, raycastMask);


                    // Create bullet trail regardless if raycast hit and quickly destroy it
                    GameObject trail = Instantiate(bulletTrailVfx, muzzle.position, Quaternion.LookRotation(ray.direction));
                    LineRenderer lineRenderer = trail.GetComponent<LineRenderer>();
                    lineRenderer.SetPosition(0, muzzle.position);

                    if (raycast)
                    {
                        //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
                        //Debug.Log("Server recognized that the object was hit: " + hit.transform.gameObject.name);

                        Player victim = hit.transform.gameObject.GetComponent<Player>();

                        if (victim)
                        {
                            // Cause damage to the victim, and pass our network ID so we can keep track of who killed who.
                            victim.TakeDamage(10, GetComponent<NetworkIdentity>().netId);
                        }
                        else
                        {
                            GameObject particles = Instantiate(bulletImpactVfx, hit.point, Quaternion.LookRotation(Vector3.Reflect(ray.direction, hit.normal)));
                            NetworkServer.Spawn(particles);
                        }

                        lineRenderer.SetPosition(1, hit.point);
                    }
                    else
                    {
                        lineRenderer.SetPosition(1, ray.direction * 10000f);
                    }

                    NetworkServer.Spawn(trail);
                    Destroy(trail, Time.deltaTime);

                    break;

                case WeaponType.Projectile:
                    // TODO: Implement projectile weapon firing
                    break;
            }

            RpcOnFire();

            if (!CurrentWeapon.weaponDefinition.silencer)
            {
                // Show muzzle flash for split second
                StartCoroutine(FlashMuzzle(influence));
            }

            // Apply recoil
            InstabilityPercentage = Mathf.Min(1f, InstabilityPercentage + CurrentWeapon.weaponDefinition.recoilMultiplier);
            // Camera recoil
            float recoil = CurrentWeapon.weaponDefinition.recoilMultiplier + 1f;
            //Player.localPlayer.verticalLook += -recoil;
            //Player.localPlayer.horizontalLook += Random.Range(-recoil, recoil);

            // Subtract ammo
            CurrentWeapon.CmdConsumeMagazine(1);

            // Update listeners
            EventOnAmmoChange?.Invoke();
        }

        [ClientRpc]
        void RpcOnFire()
        {
            // Call a method on the PlayerAnimator.cs here. (Set trigger shoot?)
        }

        /// <summary>
        /// Reloads currently equipped weapon.
        /// </summary>
        /// <returns>Reload state</returns>
        public IEnumerator ReloadWeapon()
        {
            // Already reloading or not in idle state
            if (CurrentState == WeaponState.Reloading)
            {
                yield break;
            }

            _reloadInterrupt = true;
            yield return new WaitUntil(() => CurrentState == WeaponState.Idle);

            // No more ammo
            if (CurrentWeapon.reserves == 0)
            {
                Debug.Log("No more ammo in reserves!");
                StartCoroutine(EventOnAlert?.Invoke("Out of ammo", 1f));
                _reloadInterrupt = false;
                yield break;
            }

            // Weapon already fully reloaded
            if (CurrentWeapon.magazine >= CurrentWeapon.weaponDefinition.clipSize)
            {
                Debug.Log("Magazine fully loaded, no need to reload.");
                StartCoroutine(EventOnAlert?.Invoke("Magazine full", 1f));
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
            _weaponHolderAnimator.SetFloat("ReloadSpeed", 1.0f / CurrentWeapon.weaponDefinition.reloadTime);
            //playerAnimator.Animator.SetTrigger("Reload_t");

            yield return new WaitForSeconds(CurrentWeapon.weaponDefinition.reloadTime);

            // Fill up magazine with ammo from reserves
            CurrentWeapon.CmdReloadMagazine();

            // Update listeners
            EventOnAmmoChange?.Invoke();

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

            // Play holster animation
            CmdHolsterAnimation();

            // Change the weapon
            _currentWeaponIndex = index;
            UpdateWeaponModel();
            CmdUpdateRemoteWeaponModel();
            UpdateAnimatorOverride();

            // Update listeners
            EventOnAmmoChange?.Invoke();
            EventOnWeaponChange?.Invoke();

            // Play ready animation
            CmdReadyAnimation();

            CurrentState = WeaponState.Idle;
        }

        /// <summary>
        /// Switch to the next weapon by incrementing index. If index is out of bounds, wrap to other end of array.
        /// This operation works both ways.
        /// </summary>
        public void CycleWeapons(int direction = 1)
        {
            int index = (((_currentWeaponIndex + direction) % heldWeapons.Count) + heldWeapons.Count) % heldWeapons.Count;
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
            AimingPercentage = Mathf.Clamp01(Mathf.MoveTowards(AimingPercentage, aim, 1f / CurrentWeapon.weaponDefinition.aimTime * Time.deltaTime));

            if (_weaponHolderAnimator.isActiveAndEnabled)
            {
                // Update animator to show aiming transition
                _weaponHolderAnimator.SetFloat("AimPercentage", AimingPercentage);
            }
        }

        /// <summary>
        /// Fill up all weapons to max ammo and reserves.
        /// </summary>
        public void RefillAmmo()
        {
            foreach (WeaponItem weaponItem in heldWeapons)
            {
                weaponItem.CmdFillUpAmmo();
            }

            EventOnAmmoChange?.Invoke();
        }

        /// <summary>
        /// Sets all weapon items in heldWeapons to null. Resets currently equipped weapon index to 0.
        /// </summary>
        /// <returns>Array of weapons that were removed</returns>
        public SyncListWeaponItem UnequipAllWeapons()
        {
            var weapons = heldWeapons;
            for (int i = 0; i < heldWeapons.Count; i++)
            {
                heldWeapons[i] = null;
            }

            _currentWeaponIndex = 0;
            UpdateWeaponModel();
            CmdUpdateRemoteWeaponModel();
            EventOnWeaponChange?.Invoke();
            EventOnAmmoChange?.Invoke();

            return weapons;
        }

        /// <summary>
        /// Generate Vector3 influence using weapon accuracy which will sway the bullet trajectory.
        /// </summary>
        /// <returns>Influence</returns>
        private Vector3 CalculateAccuracyInfluence()
        {
            // Cache variables for repeated access
            Transform cameraTransform = _camera.transform;

            // Accuracy reduction when not aiming
            float reduction = CurrentWeapon.weaponDefinition.accuracy * accuracyReduction * (1f - AimingPercentage);
            float accuracy = CurrentWeapon.weaponDefinition.accuracy - reduction;

            // Divide by 4 to reduce the max angle from 180 degrees to 45 degrees
            return (cameraTransform.right * Random.Range(-1f + accuracy, 1f - accuracy) + cameraTransform.up * Random.Range(-1f + accuracy, 1f - accuracy)) / 4f;
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

            // Destroy all children
            foreach (Transform child in weaponHolder)
            {
                Destroy(child.gameObject);
            }

            if (CurrentWeapon != null && CurrentWeapon.weaponDefinition != null && CurrentWeapon.weaponDefinition.modelPrefab != null)
            {
                // Spawn weapon model
                GameObject weaponModel = Instantiate(CurrentWeapon.weaponDefinition.modelPrefab, weaponHolder);
                // Set muzzle transform. The child object must be called Muzzle
                muzzle = weaponModel.transform.Find("Muzzle");
                muzzleFlash = muzzle.transform.GetChild(0);

                weaponModel.SetActive(isLocalPlayer);
            }
        }

        [Command]
        private void CmdUpdateRemoteWeaponModel()
        {
            // Reset remote muzzle transform
            remoteMuzzle = null;

            // Destroy all children
            foreach (Transform child in rightHand)
            {
                NetworkServer.Destroy(child.gameObject);
            }

            if (CurrentWeapon != null)
            {
                if (CurrentWeapon.weaponDefinition != null && CurrentWeapon.weaponDefinition.remoteModelPrefab != null)
                {
                    GameObject remoteModel = Instantiate(CurrentWeapon.weaponDefinition.remoteModelPrefab, rightHand);
                    remoteMuzzle = remoteModel.transform.Find("Muzzle");
                    NetworkServer.Spawn(remoteModel);
                    remoteModel.SetActive(!isLocalPlayer);
                }
            }
        }

        [Command]
        public void CmdUpdateAnimatorWeaponType()
        {
            RpcOnUpdateAnimatorWeaponType();
        }

        [ClientRpc]
        private void RpcOnUpdateAnimatorWeaponType()
        {
            if (CurrentWeapon == null || CurrentWeapon.weaponDefinition == null)
            {
                //playerAnimator.Animator.SetInteger("WeaponType_int", 0);
                //playerAnimator.Animator.SetBool("FullAuto_b", false);
                return;
            }

            //playerAnimator.Animator.SetInteger("WeaponType_int", CurrentWeapon.WeaponDefinition.WeaponClass.AnimatorType);
            //playerAnimator.Animator.SetBool("FullAuto_b", CurrentWeapon.WeaponDefinition.TriggerType == TriggerType.Auto);
        }

        private void UpdateAnimatorOverride()
        {
            // Update animator override or reset to default animator controller
            if (CurrentWeapon != null && CurrentWeapon.weaponDefinition != null)
            {
                var overrideController = _weaponHolderAnimator.runtimeAnimatorController as AnimatorOverrideController;
                if (CurrentWeapon.weaponDefinition.animatorOverride != null)
                {
                    _weaponHolderAnimator.runtimeAnimatorController = CurrentWeapon.weaponDefinition.animatorOverride;
                }
                else if (overrideController != null)
                {
                    _weaponHolderAnimator.runtimeAnimatorController = overrideController.runtimeAnimatorController;
                }
            }
        }

        /// <summary>
        /// Play the weapon holster animation for the duration of the holster time defined in the weapon definition.
        /// </summary>
        /// <returns>Holster animation</returns>
        private IEnumerator HolsterAnimation()
        {
            if (CurrentWeapon == null || CurrentWeapon.weaponDefinition == null)
            {
                yield return null;
            }

            // Start playing animation for holster time
            _weaponHolderAnimator.SetFloat("HolsterSpeed", 1.0f / CurrentWeapon.weaponDefinition.holsterTime);
            _weaponHolderAnimator.SetBool("Holster", true);

            // Wait for animation to finish
            yield return new WaitForSeconds(CurrentWeapon.weaponDefinition.holsterTime);

            // Reset animator parameters
            _weaponHolderAnimator.SetBool("Holster", false);
            _weaponHolderAnimator.SetFloat("HolsterSpeed", 0f);
        }

        [Command]
        private void CmdHolsterAnimation()
        {
            StartCoroutine(HolsterAnimation());
        }

        /// <summary>
        /// Play the weapon ready animation for the duration of the ready time defined in the weapon definition.
        /// </summary>
        /// <returns>Ready animation</returns>
        private IEnumerator ReadyAnimation()
        {
            if (CurrentWeapon == null || CurrentWeapon.weaponDefinition == null)
            {
                yield break;
            }

            CurrentState = WeaponState.Switching;
            // Start playing animation for ready time
            _weaponHolderAnimator.SetFloat("ReadySpeed", 1.0f / CurrentWeapon.weaponDefinition.readyTime);
            _weaponHolderAnimator.SetBool("Ready", true);

            // Wait for animation to finish
            yield return new WaitForSeconds(CurrentWeapon.weaponDefinition.readyTime);

            // Reset animator parameters
            _weaponHolderAnimator.SetBool("Ready", false);
            _weaponHolderAnimator.SetFloat("ReadySpeed", 0f);
            CurrentState = WeaponState.Idle;
        }

        [Command]
        private void CmdReadyAnimation()
        {
            StartCoroutine(ReadyAnimation());
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
