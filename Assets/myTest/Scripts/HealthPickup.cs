using Photon.Pun;

namespace myTest
{ 
    public class HealthPickup : ItemPickup
    {
        public int healAmount = 40;

        public override string ItemName => $"{healAmount} Health";

        public override void GrantPickup(PickupBehavior pickupBehavior)
        {
            Player1 player = pickupBehavior.gameObject.GetComponent<Player1>();
          //  if (player != null) player.Heal(healAmount);
           // NetworkServer.Destroy(gameObject);
            Destroy(gameObject);
        }
    }
}
