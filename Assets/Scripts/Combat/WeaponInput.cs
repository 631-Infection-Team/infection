using UnityEngine;

namespace Infection.Combat
{
    [RequireComponent(typeof(Weapon))]
    public class WeaponInput : MonoBehaviour
    {
        private Weapon m_Weapon = null;

        private void Awake()
        {
            m_Weapon = GetComponent<Weapon>();
        }

        private void Update()
        {
            if (m_Weapon.CurrentWeapon.WeaponDefinition)
            {
                switch (m_Weapon.CurrentWeapon.WeaponDefinition.TriggerType)
                {
                    case TriggerType.Auto:
                        // Automatic fire is the same as burst
                    case TriggerType.Burst:
                        // Currently you can hold down Fire to fire burst mode weapons
                        if (Input.GetButton("Fire"))
                        {
                            StartCoroutine(m_Weapon.FireWeapon());
                        }
                        break;

                    case TriggerType.Manual:
                        // Manual fire
                        if (Input.GetButtonDown("Fire"))
                        {
                            StartCoroutine(m_Weapon.FireWeapon());
                        }
                        break;
                }

                // Reload weapon
                if (Input.GetButtonDown("Reload"))
                {
                    StartCoroutine(m_Weapon.ReloadWeapon());
                }

                // Aiming down the sights
                if (Input.GetButton("Aim"))
                {
                }
            }
        }
    }
}
