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
<<<<<<< Updated upstream
=======

            if (currentWeapon)
            {
                m_magazine = currentWeapon.ClipSize;
                m_reserves = currentWeapon.MaxReserves;
            }
>>>>>>> Stashed changes
        }

        private void Update()
        {
<<<<<<< Updated upstream
            if (Input.GetButtonDown("Fire"))
            {
                FireWeapon();
=======
            if (currentWeapon)
            {
                // Automatic fire
                if (Input.GetButton("Fire"))
                {
                    if (Time.time - m_timeSinceFire > 1f / currentWeapon.FireRate)
                    {
                        m_timeSinceFire = Time.time;
                        FireWeapon();
                    }
                }

                // Stop firing
                if (Input.GetButtonUp("Fire") && (m_currentState != WeaponState.Reloading || m_currentState != WeaponState.Switching))
                {
                    m_currentState = WeaponState.Idle;
                }

                if (Input.GetButtonDown("Reload"))
                {
                    if (m_currentState == WeaponState.Idle)
                    {
                        StartCoroutine(ReloadWeapon());
                    }
                }
>>>>>>> Stashed changes
            }
        }

        private void FireWeapon()
        {
<<<<<<< Updated upstream
            if (Physics.Raycast(m_CameraController.currentCamera.transform.position, m_CameraController.currentCamera.transform.forward, out var hit, range))
            {
                if (weaponDefinition)
                {
                    Debug.Log(weaponDefinition.WeaponName + " hit target " + hit.transform.name);
=======
            if (currentWeapon)
            {
                if (m_currentState == WeaponState.Reloading || m_currentState == WeaponState.Switching)
                {
                    return;
                }

                if (m_magazine <= 0)
                {
                    if (m_reserves <= 0)
                    {
                        Debug.Log("Out of ammo!");
                        // TODO: Handle the case. Maybe switch weapon?
                        return;
                    }

                    StartCoroutine(ReloadWeapon());
                }

                if (Physics.Raycast(m_CameraController.currentCamera.transform.position, m_CameraController.currentCamera.transform.forward, out var hit, range))
                {
                    if (currentWeapon)
                    {
                        m_currentState = WeaponState.Firing;
                        Debug.Log(currentWeapon.WeaponName + " hit target " + hit.transform.name);
                    }
>>>>>>> Stashed changes
                }

                m_magazine--;
            }
<<<<<<< Updated upstream
=======

        }

        private IEnumerator ReloadWeapon()
        {
            if (currentWeapon)
            {
                if (m_currentState == WeaponState.Reloading)
                {
                    yield break;
                }

                m_currentState = WeaponState.Reloading;

                yield return new WaitForSeconds(currentWeapon.ReloadTime);

                int ammoToAdd = m_reserves -= currentWeapon.ClipSize - m_magazine;
                m_magazine += ammoToAdd;

                m_currentState = WeaponState.Idle;
            }
>>>>>>> Stashed changes
        }
    }
}
