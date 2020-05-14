using Infection.Combat;
using Mirror;

namespace Infection
{
    public static class WeaponItemSerializer
    {
        public static void WriteWeaponItem(this NetworkWriter writer, WeaponItem weaponItem)
        {
            // no need to serialize the data, just the name (asset path, not the weapon name)
            writer.WriteWeaponDefinition(weaponItem.weaponDefinition);
            writer.WritePackedInt32(weaponItem.magazine);
            writer.WritePackedInt32(weaponItem.reserves);
        }

        public static WeaponItem ReadWeaponItem(this NetworkReader reader)
        {
            // load by name.  The data will come from the asset in Resources folder
            var weaponDefinition = reader.ReadWeaponDefinition();
            var magazine = reader.ReadPackedInt32();
            var reserves = reader.ReadPackedInt32();

            return new WeaponItem(weaponDefinition, magazine, reserves);
        }
    }
}
