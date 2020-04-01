using System.IO;
using UnityEditor;
using UnityEngine;

namespace Infection.Combat
{
    public enum TriggerType
    {
        Auto,
        Burst,
        Manual
    }

    public enum WeaponType
    {
        Raycast,
        Projectile
    }

    /// <summary>
    /// Weapon definition is the blueprint of the weapon. We define all of the weapon stats here.
    /// </summary>
    [CreateAssetMenu(menuName = "Infection/Combat/Weapon")]
    public class WeaponDefinition : ScriptableObject
    {
        [Header("Weapon Properties")]
        [SerializeField]
        private string weaponName = "New Weapon";
        [SerializeField]
        private WeaponClass weaponClass = null;
        [SerializeField]
        private TriggerType triggerType = TriggerType.Auto;
        [SerializeField]
        private WeaponType weaponType = WeaponType.Raycast;
        [SerializeField, Tooltip("If the weapon has a silencer, muzzle flash will not display")]
        private bool silencer = false;

        [Header("Firing")]
        [SerializeField]
        private float damage = 8.0f;
        [SerializeField, Tooltip("Time between shots in seconds, lower is faster"), Range(0.001f, 10.0f)]
        private float fireRate = 0.1f;
        [SerializeField, Tooltip("Percentage of how accurate each shot fired will hit its mark"), Range(0f, 1f)]
        private float accuracy = 0.995f;
        [SerializeField, Tooltip("Amount of recoil from each shot, higher is more intense"), Range(0.001f, 10.0f)]
        private float recoilMultiplier = 0.1f;

        [Header("Special Action")]
        [SerializeField, Tooltip("Amount of magnification in field of view when aiming"), Range(0.001f, 10.0f)]
        private float aimZoomMultiplier = 1.10f;

        [Header("Timers")]
        [SerializeField, Tooltip("Time to reload current weapon, lower is faster"), Range(0.001f, 10.0f)]
        private float reloadTime = 2.0f;
        [SerializeField, Tooltip("Time to pull out weapon, lower is faster"), Range(0.001f, 10.0f)]
        private float readyTime = 0.5f;
        [SerializeField, Tooltip("Time to put away weapon, lower is faster"), Range(0.001f, 10.0f)]
        private float holsterTime = 0.7f;
        [SerializeField, Tooltip("Time to aim down the sights, lower is faster"), Range(0.001f, 10.0f)]
        private float aimTime = 0.2f;

        [Header("Ammunition")]
        [SerializeField, Tooltip("Maximum number of rounds in the magazine")]
        private int clipSize = 30;
        [SerializeField, Tooltip("Maximum number of rounds kept in reserves")]
        private int maxReserves = 120;

        [Header("Rendering")]
        [SerializeField, Tooltip("Crosshair specific to this weapon")]
        private Sprite crosshair = null;
        [SerializeField, Tooltip("Prefab for the weapon model with predefined transforms")]
        private GameObject modelPrefab = null;
        [SerializeField, Tooltip("Override controller for animations used by WeaponHolder animator")]
        private AnimatorOverrideController animatorOverride = null;

        public string WeaponName => weaponName;
        public WeaponClass WeaponClass => weaponClass;
        public TriggerType TriggerType => triggerType;
        public WeaponType WeaponType => weaponType;
        public bool Silencer => silencer;
        public float Damage => damage;
        public float FireRate => fireRate;
        public float Accuracy => accuracy;
        public float RecoilMultiplier => recoilMultiplier;
        public float AimZoomMultiplier => aimZoomMultiplier;

        public float ReloadTime => reloadTime;
        public float ReadyTime => readyTime;
        public float HolsterTime => holsterTime;
        public float AimTime => aimTime;

        public int ClipSize => clipSize;
        public int MaxReserves => maxReserves;
        public Sprite Crosshair => crosshair;
        public GameObject ModelPrefab => modelPrefab;
        public AnimatorOverrideController AnimatorOverride => animatorOverride;

        /// <summary>
        /// The time it takes to empty a full magazine without reloading or stopping.
        /// </summary>
        public float TimeToEmptyMagazine => ClipSize * FireRate;

        /// <summary>
        /// The time it takes to fully reload all magazines until reserves is empty provided
        /// that all magazines are consumed from full to empty without partial reloading.
        /// </summary>
        public float TimeToReloadAllReserves => ReloadTime * NumberOfMagazines;

        /// <summary>
        /// The time it takes to empty all magazines taking into account reload time and fire rate.
        /// </summary>
        public float TimeToEmptyEntireWeapon => TimeToEmptyMagazine * NumberOfMagazines + TimeToReloadAllReserves;

        /// <summary>
        /// The number of full magazines from reserves including the starting magazine.
        /// </summary>
        public float NumberOfMagazines => 1 + (float) MaxReserves / ClipSize;

        /// <summary>
        /// Change weapon name to match file name when renaming asset.
        /// </summary>
        private void OnValidate()
        {
#if UNITY_EDITOR
            weaponName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(this));
#endif
        }

        /// <summary>
        /// The amount of ammo consumed in a single magazine throughout a duration without reloading or stopping.
        /// </summary>
        /// <param name="duration">How long to constantly fire the weapon for</param>
        /// <returns>Amount of ammo consumed from the magazine</returns>
        public int GetExpectedAmmoConsumed(float duration)
        {
            return (int) (1.0f / FireRate * duration);
        }

        /// <summary>
        /// The amount of ammo left in the magazine after firing for a duration without reloading or stopping.
        /// </summary>
        /// <param name="duration">How long to constantly fire the weapon for</param>
        /// <returns>Amount of ammo left in the magazine</returns>
        public int GetExpectedMagazineAfterFiringFor(float duration)
        {
            return ClipSize - GetExpectedAmmoConsumed(duration);
        }
    }
}
