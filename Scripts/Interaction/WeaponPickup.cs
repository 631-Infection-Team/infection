using Infection.Combat;
using Mirror;
using UnityEngine;

namespace Infection.Interaction
{
    public class WeaponPickup : ItemPickup
    {
        public WeaponItem weaponItem = null;

        public override string ItemName => weaponItem.weaponDefinition.weaponName;

        public WeaponItem WeaponItem
        {
            get => weaponItem;
            set => weaponItem = value;
        }

        public override void GrantPickup(PickupBehavior pickupBehavior)
        {
            if (WeaponItem == null)
            {
                return;
            }

            Weapon weapon = pickupBehavior.gameObject.GetComponent<Weapon>();
            if (weapon != null && weapon.CurrentState == Weapon.WeaponState.Idle)
            {
                Debug.Log("Picking up weapon item");
                // Equip new weapon and drop old weapon, preserve ammo
                WeaponItem oldWeapon = weapon.EquipWeapon(WeaponItem);
                Transform playerTransform = weapon.transform;

                if (oldWeapon != null)
                {
                    // Forward vector for weapon pickup object is the direction the weapon is facing, so it needs to be facing left of our player
                    Quaternion rotation = Quaternion.LookRotation(-playerTransform.right);
                    // Local rotation for dropping weapon should be 0, 0, -90, which would have it lay flat
                    rotation *= Quaternion.Euler(0f, 0f, -90f);

                    GameObject pickup = Instantiate(oldWeapon.weaponDefinition.pickupPrefab, playerTransform.position, rotation);
                    pickup.GetComponent<WeaponPickup>().WeaponItem = oldWeapon;

                    // Throw weapon up a little and forward a lot
                    // TODO: Player should not be on "Default" layer, otherwise player will collide with the object when dropping old weapon
                    pickup.GetComponent<Rigidbody>().AddRelativeForce(2f, 12f, 0f, ForceMode.Impulse);
                    NetworkServer.Spawn(pickup);
                }

                NetworkServer.Destroy(gameObject);
                Destroy(gameObject);
            }
        }
    }
}
