using UnityEngine;

namespace Infection.Combat
{
    public enum TriggerType
    {
        Auto,
        Semi,
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
        [SerializeField] private TriggerType triggerType = TriggerType.Manual;
        [SerializeField] private WeaponType weaponType = WeaponType.Raycast;
        [SerializeField] private float damage = 1.0f;
        [SerializeField, Tooltip("Time between shots in seconds")] private float fireRate = 0.1f;
        [SerializeField] private float reloadTime = 2.0f;
        [SerializeField] private int clipSize = 4;
        [SerializeField] private int maxReserves = 16;

        public string WeaponName => weaponName;
        public WeaponClass WeaponClass => weaponClass;
        public TriggerType TriggerType => triggerType;
        public WeaponType WeaponType => weaponType;
        public float Damage => damage;
        public float FireRate => fireRate;
        public float ReloadTime => reloadTime;
        public int ClipSize => clipSize;
        public int MaxReserves => maxReserves;
    }

    /// <summary>
    /// Weapon class defines the archetype for the weapon. This can be assault rifle, pistol, etc.
    /// </summary>
    [CreateAssetMenu(menuName = "Infection/Combat/Weapon Class")]
    public class WeaponClass : ScriptableObject
    {
        [SerializeField] private string weaponClassName = "New Weapon Class";

        public string WeaponClassName => weaponClassName;
    }
}
