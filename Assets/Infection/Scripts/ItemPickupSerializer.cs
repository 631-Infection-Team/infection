// using Infection.Interaction;
// using Mirror;
// using UnityEngine;
//
// namespace Infection
// {
//     public static class ItemPickupSerializer
//     {
//         private const byte WEAPON_PICKUP = 1;
//         private const byte AMMO_PICKUP = 2;
//         private const byte HEALTH_PICKUP = 3;
//
//         public static void WriteItemPickup(this NetworkWriter writer, ItemPickup itemPickup)
//         {
//             if (itemPickup is WeaponPickup weaponPickup)
//             {
//                 writer.WriteByte(WEAPON_PICKUP);
//                 writer.WriteString(weaponPickup.name);
//                 writer.WriteWeaponItem(weaponPickup.weaponItem);
//             }
//             else if (itemPickup is AmmoPickup ammoPickup)
//             {
//                 writer.WriteByte(AMMO_PICKUP);
//                 writer.WriteString(ammoPickup.name);
//             }
//             else if (itemPickup is HealthPickup healthPickup)
//             {
//                 writer.WriteByte(HEALTH_PICKUP);
//                 writer.WriteString(healthPickup.name);
//                 writer.WritePackedInt32(healthPickup.healAmount);
//             }
//         }
//
//         public static ItemPickup ReadItemPickup(this NetworkReader reader)
//         {
//             byte type = reader.ReadByte();
//             GameObject pickup = new GameObject();
//             pickup.AddComponent<MeshRenderer>();
//             switch (type)
//             {
//                 case WEAPON_PICKUP:
//                     var weaponPickup = pickup.AddComponent<WeaponPickup>();
//                     weaponPickup.name = reader.ReadString();
//                     weaponPickup.weaponItem = reader.ReadWeaponItem();
//                     return weaponPickup;
//
//                 case AMMO_PICKUP:
//                     var ammoPickup = pickup.AddComponent<AmmoPickup>();
//                     ammoPickup.name = reader.ReadString();
//                     return ammoPickup;
//
//                 case HEALTH_PICKUP:
//                     var healthPickup = pickup.AddComponent<HealthPickup>();
//                     healthPickup.name = reader.ReadString();
//                     healthPickup.healAmount = reader.ReadPackedInt32();
//                     return healthPickup;
//
//                 // case WEAPON_PICKUP:
//                 //     return new WeaponPickup
//                 //     {
//                 //         name = reader.ReadString(),
//                 //         weaponItem = reader.ReadWeaponItem()
//                 //     };
//                 //
//                 // case AMMO_PICKUP:
//                 //     return new AmmoPickup
//                 //     {
//                 //         name = reader.ReadString()
//                 //     };
//                 //
//                 // case HEALTH_PICKUP:
//                 //     return new HealthPickup
//                 //     {
//                 //         name = reader.ReadString(),
//                 //         healAmount = reader.ReadPackedInt32()
//                 //     };
//
//                 default:
//                     throw new System.Exception($"Invalid item pickup type {type}");
//             }
//         }
//     }
// }
