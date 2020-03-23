using System.Collections;
using Infection.Combat;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class WeaponTests
    {
        // A Test behaves as an ordinary method
        [Test]
        public void WeaponTestsSimplePasses()
        {
            // Use the Assert class to test conditions
            Debug.Log("Test");
        }

        [UnityTest]
        public IEnumerator FireWeaponForFiveSecondsTest()
        {
            GameObject testPlayer = new GameObject();
            Weapon weapon = testPlayer.AddComponent<Weapon>();
            // Use default values
            WeaponDefinition ak47 = ScriptableObject.CreateInstance<WeaponDefinition>();

            // Equip weapon
            weapon.EquipWeapon(new WeaponSlot(ak47, 30, 120));
            yield return new WaitUntil(() => weapon.CurrentState == Weapon.WeaponState.Idle);

            // Check if weapon is equipped and ready
            Assert.True(weapon.CurrentWeapon.weapon.Equals(ak47));
            Assert.True(weapon.CurrentState == Weapon.WeaponState.Idle);

            // Fire weapon
            yield return weapon.FireWeapon();
            yield return new WaitUntil(() => weapon.CurrentState == Weapon.WeaponState.Idle);

            // Check remaining ammo
            Assert.True(weapon.CurrentWeapon.magazine == 29);
        }

        [UnityTest]
        public IEnumerator FireWeaponUntilEmptyMagazineTest()
        {
            yield return null;
        }

        [UnityTest]
        public IEnumerator FireWeaponUntilEmptyMagazineWithNoReservesTest()
        {
            yield return null;
        }

        [UnityTest]
        public IEnumerator SwitchWeaponTest()
        {
            yield return null;
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator WeaponTestsWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            Debug.Log("Unity Test");
            yield return null;
        }
    }
}
