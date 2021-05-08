using System.IO;
using UnityEditor;
using UnityEngine;

namespace myTest.Combat
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
    [CreateAssetMenu(menuName = "myTest/Combat/Weapon")]
    public class WeaponDefinition : ScriptableObject
    {
        [Header("Weapon Properties")]
        public string weaponName = "New Weapon";
        public WeaponClass weaponClass = null;
        public TriggerType triggerType = TriggerType.Auto;
        public WeaponType weaponType = WeaponType.Raycast;
        [Tooltip("If the weapon has a silencer, muzzle flash will not display")]
        public bool silencer = false;

        [Header("Firing")]
        public int damage = 8;
        [Tooltip("Time between shots in seconds, lower is faster"), Range(0.001f, 4.0f)]
        public float fireRate = 0.1f;
        [Tooltip("Percentage of how accurate each shot fired will hit its mark"), Range(0f, 1f)]
        public float accuracy = 0.995f;
        [Tooltip("Amount of recoil from each shot, higher is more intense"), Range(0.001f, 2.0f)]
        public float recoilMultiplier = 0.1f;
        [Tooltip("Additional vertical intensity applied on top of regular recoil multiplier"), Range(0.001f, 2.0f)]
        public float verticalRecoilIntensity = 0f;

        [Header("Special Action")]
        [Tooltip("Amount of magnification in field of view when aiming"), Range(0.001f, 8.0f)]
        public float aimZoomMultiplier = 1.10f;
        [Tooltip("Prefab for the weapon used for weapon pickups")]
        public GameObject pickupPrefab = null;

        [Header("Timers")]
        [Tooltip("Time to reload current weapon, lower is faster"), Range(0.001f, 5.0f)]
        public float reloadTime = 2.0f;
        [Tooltip("Time to pull out weapon, lower is faster"), Range(0.001f, 2.0f)]
        public float readyTime = 0.5f;
        [Tooltip("Time to put away weapon, lower is faster"), Range(0.001f, 2.0f)]
        public float holsterTime = 0.7f;
        [Tooltip("Time to aim down the sights, lower is faster"), Range(0.001f, 0.25f)]
        public float aimTime = 0.2f;

        [Header("Ammunition")]
        [Tooltip("Maximum number of rounds in the magazine"), Min(0)]
        public int clipSize = 30;
        [Tooltip("Maximum number of rounds kept in reserves, -1 = infinite"), Min(-1)]
        public int maxReserves = 120;

        [Header("Rendering")]
        [Tooltip("Crosshair specific to this weapon")]
        public Sprite crosshair = null;
        [Tooltip("Prefab for the weapon model with predefined transforms")]
        public GameObject modelPrefab = null;
        [Tooltip("Prefab for the weapon model shown to other players")]
        public GameObject remoteModelPrefab = null;
        [Tooltip("Override controller for animations used by WeaponHolder animator")]
        public AnimatorOverrideController animatorOverride = null;

        /// <summary>
        /// The time it takes to empty a full magazine without reloading or stopping.
        /// </summary>
        public float TimeToEmptyMagazine => clipSize * fireRate;

        /// <summary>
        /// The time it takes to fully reload all magazines until reserves is empty provided
        /// that all magazines are consumed from full to empty without partial reloading.
        /// </summary>
        public float TimeToReloadAllReserves => reloadTime * NumberOfMagazines;

        /// <summary>
        /// The time it takes to empty all magazines taking into account reload time and fire rate.
        /// </summary>
        public float TimeToEmptyEntireWeapon => TimeToEmptyMagazine * NumberOfMagazines + TimeToReloadAllReserves;

        /// <summary>
        /// The number of full magazines from reserves including the starting magazine.
        /// </summary>
        public float NumberOfMagazines => 1 + (float)maxReserves / clipSize;

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
            return (int)(1.0f / fireRate * duration);
        }

        /// <summary>
        /// The amount of ammo left in the magazine after firing for a duration without reloading or stopping.
        /// </summary>
        /// <param name="duration">How long to constantly fire the weapon for</param>
        /// <returns>Amount of ammo left in the magazine</returns>
        public int GetExpectedMagazineAfterFiringFor(float duration)
        {
            return clipSize - GetExpectedAmmoConsumed(duration);
        }
    }
}

