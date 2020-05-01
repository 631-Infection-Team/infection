using Mirror;
using UnityEngine;

namespace Infection.Interaction
{
    public class HealthPickup : ItemPickup
    {
        [SerializeField] private int healAmount = 40;

        public override string ItemName => $"{healAmount} Health";

        protected override void OnGrantPickup(PickupBehavior pickupBehavior)
        {
            Player player = pickupBehavior.gameObject.GetComponent<Player>();
            if (player != null) player.Heal(healAmount);
            NetworkServer.Destroy(gameObject);
            Destroy(gameObject);
        }
    }
}
