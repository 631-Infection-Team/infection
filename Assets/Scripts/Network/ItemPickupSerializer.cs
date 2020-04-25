using Infection.Interaction;
using Mirror;

namespace Infection.Network
{
    public static class ItemPickupSerializer
    {
        private const byte ITEM_PICKUP_ID = 0;
        private const byte WEAPON_PICKUP_ID = 1;
        private const byte AMMO_PICKUP_ID = 2;
        private const byte HEALTH_PICKUP_ID = 3;

        public static void WriteItem(this NetworkWriter writer, ItemPickup itemPickup)
        {
            if (itemPickup is WeaponPickup weaponPickup)
            {
                writer.WriteByte(WEAPON_PICKUP_ID);
                writer.WriteString(weaponPickup.ItemName);
            }
            else if (itemPickup is AmmoPickup ammoPickup)
            {
                writer.WriteByte(WEAPON_PICKUP_ID);
                writer.WriteString(ammoPickup.ItemName);
            }
            else if (itemPickup is HealthPickup healthPickup)
            {
                writer.WriteByte(WEAPON_PICKUP_ID);
                writer.WriteString(healthPickup.ItemName);
            }
            else
            {
                writer.WriteByte(ITEM_PICKUP_ID);
                writer.WriteString(itemPickup.ItemName);
            }
        }

        public static ItemPickup ReadItem(this NetworkReader reader)
        {
            byte id = reader.ReadByte();

            switch (id)
            {
                case WEAPON_PICKUP_ID:
                case AMMO_PICKUP_ID:
                case HEALTH_PICKUP_ID:
                case ITEM_PICKUP_ID:
                default:
                    throw new System.Exception($"Unhandled item pickup type for {id}.");
            }
        }
    }
}
