using System.IO;
using UnityEditor;
using UnityEngine;

namespace Infection.Combat
{
    /// <summary>
    /// Weapon class defines the archetype for the weapon. This can be assault rifle, pistol, etc.
    /// </summary>
    [CreateAssetMenu(menuName = "Infection/Combat/Weapon Class")]
    public class WeaponClass : ScriptableObject
    {
        [SerializeField] private string weaponClassName = "New Weapon Class";

        public string WeaponClassName => weaponClassName;

        private void OnValidate()
        {

#if UNITY_EDITOR
            weaponClassName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(this));
#endif
        }
    }
}
