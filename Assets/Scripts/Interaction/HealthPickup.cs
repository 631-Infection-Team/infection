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
            if (player == null)
            {
                return;
            }

            // Heal player with lower limit of 0 and upper limit of health max
            player.health = Mathf.Clamp(player.health + healAmount, 0, player.healthMax);
        }
    }
}
