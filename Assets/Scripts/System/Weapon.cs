using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Infection
{
    public class Weapon : MonoBehaviour
    {
        static RaycastHit[] s_HitInfoBuffer = new RaycastHit[8];

        public enum TriggerType
        {
            Auto,
            Manual
        }

        public enum WeaponType
        {
            Raycast,
            Projectile
        }

        public enum WeaponState
        {
            Idle,
            Firing,
            Reloading
        }

        [System.Serializable]
        public class AdvancedSettings
        {
            public float spreadAngle = 0.0f;
            public int projectilePerShot = 1;
            public float screenShakeMultiplier = 1.0f;
        }

        public TriggerType triggerType = TriggerType.Manual;
        public WeaponType weaponType = WeaponType.Raycast;
        public float fireRate = 0.5f;
        public float reloadTime = 2.0f;
        public int clipSize = 4;
        public float damage = 1.0f;
    }
}