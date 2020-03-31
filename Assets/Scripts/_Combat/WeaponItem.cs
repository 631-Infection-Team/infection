using System;
using UnityEngine;

namespace Infection.Combat
{
    [Serializable]
    public class WeaponItem
    {
        [SerializeField] private WeaponDefinition weaponDefinition;
        [SerializeField] private int magazine;
        [SerializeField] private int reserves;

        public WeaponDefinition WeaponDefinition => weaponDefinition;
        public int Magazine => magazine;
        public int Reserves => reserves;

        public WeaponItem()
        {
            weaponDefinition = null;
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
            return Math.Min(ammoConsumed, Magazine);
        }

        public void ReloadMagazine()
        {
            // Cannot reload more ammo than how much is in reserves
            int ammoToAdd = Math.Min(WeaponDefinition.ClipSize - Magazine, Reserves);
            magazine += ammoToAdd;
            reserves -= ammoToAdd;
        }
    }
}
