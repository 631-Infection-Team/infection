using Mirror;
using UnityEngine;
using Infection.Combat;
using FMODUnity;

namespace Infection.Interaction
{
    public class AmmoPickup : ItemPickup
    {
        public override string ItemName => "Ammo";

        protected override void OnGrantPickup(PickupBehavior pickupBehavior)
        {
            Weapon weapon = pickupBehavior.gameObject.GetComponent<Weapon>();
            if (weapon == null)
            {
                return;
            }

            weapon.RefillAmmo();
            NetworkServer.Destroy(gameObject);
            Destroy(gameObject);
        }
    }
}
