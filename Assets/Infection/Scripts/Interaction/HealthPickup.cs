using Mirror;

namespace Infection.Interaction
{
    public class HealthPickup : ItemPickup
    {
        public int healAmount = 40;

        public override string ItemName => $"{healAmount} Health";

        public override void GrantPickup(PickupBehavior pickupBehavior)
        {
            Player player = pickupBehavior.gameObject.GetComponent<Player>();
            if (player != null) player.Heal(healAmount);
            NetworkServer.Destroy(gameObject);
            Destroy(gameObject);
        }
    }
}
