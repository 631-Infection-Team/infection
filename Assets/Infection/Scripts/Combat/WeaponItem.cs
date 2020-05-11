using System;
using Mirror;

namespace Infection.Combat
{
    [Serializable]
    public class WeaponItem
    {
        [SyncVar] public WeaponDefinition weaponDefinition;
        [SyncVar] public int magazine;
        [SyncVar] public int reserves;

        public WeaponItem()
        {
            weaponDefinition = null;
            magazine = 0;
            reserves = 0;
        }

        public WeaponItem(WeaponItem other)
        {
            weaponDefinition = other.weaponDefinition;
            magazine = other.magazine;
            reserves = other.reserves;
        }

        public WeaponItem(WeaponDefinition weaponDefinition)
        {
            this.weaponDefinition = weaponDefinition;
            magazine = 0;
            reserves = 0;
        }

        public WeaponItem(WeaponDefinition weaponDefinition, int magazine, int reserves)
        {
            this.weaponDefinition = weaponDefinition;
            this.magazine = magazine;
            this.reserves = reserves;
        }

        public int ConsumeMagazine(int ammoConsumed = 1)
        {
            magazine = Math.Max(0, magazine - ammoConsumed);
            return Math.Min(ammoConsumed, magazine);
        }

        public void ReloadMagazine()
        {
            // Weapon definition defined as infinite reserves
            if (weaponDefinition.maxReserves < 0)
            {
                magazine = weaponDefinition.clipSize;
                return;
            }

            // Cannot reload more ammo than how much is in reserves
            int ammoToAdd = Math.Min(weaponDefinition.clipSize - magazine, reserves);
            magazine += ammoToAdd;
            reserves -= ammoToAdd;
        }

        /// <summary>
        /// Fill up magazine to max clip size and reserves to max reserves.
        /// </summary>
        public void FillUpAmmo()
        {
            if (weaponDefinition == null)
            {
                return;
            }
            magazine = weaponDefinition.clipSize;
            reserves = weaponDefinition.maxReserves;
        }
    }

    [Serializable]
    public class SyncListWeaponItem : SyncList<WeaponItem>
    {
    }
}
