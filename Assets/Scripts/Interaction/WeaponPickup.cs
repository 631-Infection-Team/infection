using Infection.Combat;
using UnityEngine;

namespace Infection.Interaction
{
    public class WeaponPickup : ItemPickup
    {
        [SerializeField] private WeaponItem weaponItem = null;

        public override string ItemName => weaponItem.WeaponDefinition.WeaponName;

        public WeaponItem WeaponItem
        {
            get => weaponItem;
            set => weaponItem = value;
        }

        protected override void OnGrantPickup(PickupBehavior pickupBehavior)
        {
            if (WeaponItem == null)
            {
                return;
            }

            Weapon playerWeapon = pickupBehavior.gameObject.GetComponent<Weapon>();
            if (playerWeapon != null && playerWeapon.CurrentState == Weapon.WeaponState.Idle)
            {
                // Equip new weapon and drop old weapon, preserve ammo
                WeaponItem oldWeapon = playerWeapon.EquipWeapon(WeaponItem);
                Transform playerTransform = playerWeapon.transform;

                if (oldWeapon != null)
                {
                    // Forward vector for weapon pickup object is the direction the weapon is facing, so it needs to be facing left of our player
                    Quaternion rotation = Quaternion.LookRotation(-playerTransform.right);
                    // Local rotation for dropping weapon should be 0, 0, -90, which would have it lay flat
                    rotation *= Quaternion.Euler(0f, 0f, -90f);

                    GameObject pickup = Instantiate(oldWeapon.WeaponDefinition.PickupPrefab, playerTransform.position, rotation);
                    pickup.GetComponent<WeaponPickup>().WeaponItem = oldWeapon;

                    // Throw weapon up a little and forward a lot
                    // TODO: Player should not be on "Default" layer, otherwise player will collide with the object when dropping old weapon
                    pickup.GetComponent<Rigidbody>().AddRelativeForce(2f, 5f, 0f, ForceMode.Impulse);
                }
            }
        }
    }
}
