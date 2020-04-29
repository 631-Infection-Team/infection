using Infection.Combat;
using Mirror;
using UnityEngine;

namespace Infection
{
    public static class WeaponDefinitionSerializer
    {
        public static void WriteWeaponDefinition(this NetworkWriter writer, WeaponDefinition weaponDefinition)
        {
            // no need to serialize the data, just the name of the armor
            writer.WriteString(weaponDefinition.name);
        }

        public static WeaponDefinition ReadWeaponDefinition(this NetworkReader reader)
        {
            // load the same armor by name.  The data will come from the asset in Resources folder
            return Resources.Load<WeaponDefinition>(reader.ReadString());
        }
    }
}
