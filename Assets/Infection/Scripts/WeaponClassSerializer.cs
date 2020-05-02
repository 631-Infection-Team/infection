using Infection.Combat;
using Mirror;
using UnityEngine;

namespace Infection
{
    public static class WeaponClassSerializer
    {
        public static void WriteWeaponClass(this NetworkWriter writer, WeaponClass weaponClass)
        {
            // no need to serialize the data, just the name of the armor
            writer.WriteString(weaponClass.name);
        }

        public static WeaponClass ReadWeaponClass(this NetworkReader reader)
        {
            // load the same armor by name.  The data will come from the asset in Resources folder
            return Resources.Load<WeaponClass>(reader.ReadString());
        }
    }
}
