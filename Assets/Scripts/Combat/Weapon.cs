using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Infection.Combat
{
    public class Weapon : MonoBehaviour
    {
        static RaycastHit[] s_HitInfoBuffer = new RaycastHit[8];

        public enum WeaponState
        {
            Idle,
            Firing,
            Reloading,
            Switching
        }

        [SerializeField] private WeaponDefinition weaponDefinition = null;

        public Camera mainCamera = null;
        public float range = 100f;

        private void Update()
        {
            if (Input.GetButtonDown("Fire"))
            {
                FireWeapon();
            }
        }

        private void FireWeapon()
        {
            if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out var hit, range))
            {
                Debug.Log(weaponDefinition.WeaponName + " hit target " + hit.transform.name);
            }
        }
    }
}
