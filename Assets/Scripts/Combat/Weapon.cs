using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Infection.Combat
{
    public class Weapon : MonoBehaviour
    {
        [SerializeField] private WeaponDefinition currentWeapon = null;
        [SerializeField] private float range = 100f;

        private CameraController m_CameraController = null;
        private WeaponState currentState = WeaponState.Idle;
        private bool aimingDownSights = false;
        private int magazine = 0;
        private int reserves = 0;
        private float timeSinceFire = Mathf.Infinity;

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

            if (currentWeapon)
            {
                magazine = currentWeapon.ClipSize;
                reserves = currentWeapon.MaxReserves;
            }
        }

        private void Update()
        {
            if (currentWeapon)
            {
                // Automatic fire
                if (Input.GetButton("Fire"))
                {
                    if (Time.time - timeSinceFire > 1f / currentWeapon.FireRate)
                    {
                        timeSinceFire = Time.time;
                        FireWeapon();
                    }
                }

                // Stop firing
                if (Input.GetButtonUp("Fire") && (currentState != WeaponState.Reloading || currentState != WeaponState.Switching))
                {
                    currentState = WeaponState.Idle;
                }

                if (Input.GetButtonDown("Reload"))
                {
                    if (currentState == WeaponState.Idle)
                    {
                        StartCoroutine(ReloadWeapon());
                    }
                }
            }
        }

        private void FireWeapon()
        {
            if (currentWeapon)
            {
                if (currentState == WeaponState.Reloading || currentState == WeaponState.Switching)
                {
                    return;
                }

                if (magazine <= 0)
                {
                    if (reserves <= 0)
                    {
                        Debug.Log("Out of ammo!");
                        // TODO: Handle the case. Maybe switch weapon?
                        return;
                    }

                    StartCoroutine(ReloadWeapon());
                }

                if (Physics.Raycast(m_CameraController.currentCamera.transform.position, m_CameraController.currentCamera.transform.forward, out var hit, range))
                {
                    currentState = WeaponState.Firing;
                    Debug.Log(currentWeapon.WeaponName + " hit target " + hit.transform.name);
                }

                magazine--;
            }
        }

        private IEnumerator ReloadWeapon()
        {
            if (currentWeapon)
            {
                if (currentState == WeaponState.Reloading)
                {
                    yield break;
                }

                currentState = WeaponState.Reloading;

                yield return new WaitForSeconds(currentWeapon.ReloadTime);

                int ammoToAdd = reserves -= currentWeapon.ClipSize - magazine;
                magazine += ammoToAdd;

                currentState = WeaponState.Idle;

                magazine--;
            }
        }
    }
}
