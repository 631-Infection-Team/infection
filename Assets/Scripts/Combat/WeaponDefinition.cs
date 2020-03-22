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
        [SerializeField] private string weaponName = "New Weapon";
        [SerializeField] private WeaponClass weaponClass = null;
        [SerializeField] private TriggerType triggerType = TriggerType.Auto;
        [SerializeField] private WeaponType weaponType = WeaponType.Raycast;
        [SerializeField] private float damage = 8.0f;
        [SerializeField, Tooltip("Time between shots in seconds")] private float fireRate = 0.1f;
        [SerializeField] private float reloadTime = 2.0f;
        [SerializeField, Tooltip("Time to pull out weapon")] private float readyTime = 0.5f;
        [SerializeField, Tooltip("Time to put away weapon")] private float holsterTime = 0.7f;
        [SerializeField] private int clipSize = 30;
        [SerializeField] private int maxReserves = 120;

        public string WeaponName => weaponName;
        public WeaponClass WeaponClass => weaponClass;
        public TriggerType TriggerType => triggerType;
        public WeaponType WeaponType => weaponType;
        public float Damage => damage;
        public float FireRate => fireRate;
        public float ReloadTime => reloadTime;
        public float ReadyTime => readyTime;
        public float HolsterTime => holsterTime;
        public int ClipSize => clipSize;
        public int MaxReserves => maxReserves;

        /// <summary>
        /// Change weapon name to match file name when renaming asset.
        /// </summary>
        private void OnValidate()
        {
#if UNITY_EDITOR
            weaponName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(this));
#endif
        }
    }
}
