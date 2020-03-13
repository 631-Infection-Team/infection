using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Infection.Combat
{
    public class Weapon : MonoBehaviour
    {
        private CameraController m_CameraController;
        [SerializeField] private WeaponDefinition weaponDefinition = null;
        public float range = 100f;

        public enum WeaponState
        {
            Idle,
            Firing,
            Reloading,
            Switching
        }

        private void Start()
        {
            m_CameraController = GetComponent<CameraController>();
        }

        private void Update()
        {
            if (Input.GetButtonDown("Fire"))
            {
                FireWeapon();
            }
        }

        private void FireWeapon()
        {
            if (Physics.Raycast(m_CameraController.currentCamera.transform.position, m_CameraController.currentCamera.transform.forward, out var hit, range))
            {
                Debug.Log(weaponDefinition.WeaponName + " hit target " + hit.transform.name);
            }
        }
    }
}
