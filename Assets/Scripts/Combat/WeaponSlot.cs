using System;

namespace Infection.Combat
{
    [Serializable]
    public class WeaponSlot
    {
        public WeaponDefinition weapon;
        public int magazine;
        public int reserves;

        public WeaponSlot(WeaponDefinition weapon, int magazine, int reserves)
        {
            this.weapon = weapon;
            this.magazine = magazine;
            this.reserves = reserves;
        }
    }
}
