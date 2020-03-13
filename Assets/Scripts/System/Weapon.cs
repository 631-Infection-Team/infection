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

    public TriggerType triggerType = TriggerType.Manual;
    public WeaponType weaponType = WeaponType.Raycast;
    public float fireRate = 0.5f;
    public float reloadTime = 2.0f;
    public int clipSize = 4;
    public float damage = 1.0f;
    public Camera camera = null;
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
        if (Physics.Raycast(camera.transform.position, camera.transform.forward, out var hit, range))
        {
            Debug.Log(hit.transform.name);
        }
    }
}
