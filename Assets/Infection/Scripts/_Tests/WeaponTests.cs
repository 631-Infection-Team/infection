using System;
using System.Collections;
using Infection.Combat;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class WeaponTests
    {
        private Weapon weapon = null;
        private WeaponDefinition defaultWeapon = null;

        // // A Test behaves as an ordinary method
        // [Test]
        // public void WeaponTestsSimplePasses()
        // {
        //     // Use the Assert class to test conditions
        //     Debug.Log("Test");
        // }

        private void Init()
        {
            weapon = new GameObject("Test Player").AddComponent<Weapon>();
            // Use default values
            defaultWeapon = ScriptableObject.CreateInstance<WeaponDefinition>();
        }

        private IEnumerator EquipWeapon()
        {
            // Equip new weapon
            weapon.EquipWeapon(new WeaponItem(defaultWeapon, 30, 120));
            yield return new WaitUntil(() => weapon.CurrentState == Weapon.WeaponState.Idle);
        }

        [UnityTest]
        public IEnumerator EquipOneWeapon()
        {
            Init();
            yield return EquipWeapon();

            // Check if weapon is equipped and ready
            Assert.True(weapon.CurrentWeapon.weaponDefinition.Equals(defaultWeapon));
            Assert.True(weapon.CurrentState == Weapon.WeaponState.Idle);
        }

        [UnityTest]
        public IEnumerator EquipTwoWeaponsTest()
        {
            Init();
            yield return EquipWeapon();
            yield return EquipWeapon();

            // Check if player cannot hold anymore weapons
            Assert.True(weapon.IsFullOfWeapons);
        }

        [UnityTest]
        public IEnumerator EquipTwoWeaponsThenFireOnceAndReplaceOldWeaponTest()
        {
            Init();
            yield return EquipWeapon();
            yield return EquipWeapon();

            // This test equips two weapons so that the player cannot hold anymore weapons.
            // Then fires a round from the second equipped weapon.
            // Finally equip a new weapon which would replace the weapon that just fired.

            // Fire weapon
            yield return weapon.FireWeapon();
            yield return new WaitUntil(() => weapon.CurrentState == Weapon.WeaponState.Idle);

            // Equip new weapon and replace old weapon
            yield return EquipWeapon();

            // Check remaining ammo
            Debug.Log("Magazine");
            Debug.Log("Result: " + weapon.CurrentWeapon.magazine);
            Debug.Log("Expected: " + weapon.CurrentWeapon.weaponDefinition.clipSize);
            Assert.True(weapon.CurrentWeapon.magazine == weapon.CurrentWeapon.weaponDefinition.clipSize);
        }

        [UnityTest]
        public IEnumerator FireWeaponOnce()
        {
            Init();
            yield return EquipWeapon();

            // Fire weapon
            yield return weapon.FireWeapon();
            yield return new WaitUntil(() => weapon.CurrentState == Weapon.WeaponState.Idle);

            // Check remaining ammo
            Debug.Log("Magazine");
            Debug.Log("Result: " + weapon.CurrentWeapon.magazine);
            Debug.Log("Expected: " + (weapon.CurrentWeapon.weaponDefinition.clipSize - 1));
            Assert.True(weapon.CurrentWeapon.magazine == weapon.CurrentWeapon.weaponDefinition.clipSize - 1);
        }

        [UnityTest]
        public IEnumerator FireWeaponForTwoSecondsTest()
        {
            Init();
            yield return EquipWeapon();

            // 0.1 fire rate = 600 RPM (0.1 seconds between each shot)
            // Firing for 2 seconds should consume about 18~20 rounds
            // Should have 10~12 rounds left in magazine after this test

            float duration = 2.0f;
            float start = Time.time;
            // Fire weapon for 2 seconds
            while (Time.time < start + duration)
            {
                yield return weapon.FireWeapon();
            }

            // Check remaining ammo
            int expected = weapon.CurrentWeapon.weaponDefinition.GetExpectedMagazineAfterFiringFor(duration);
            Debug.Log("Magazine");
            Debug.Log("Result: " + weapon.CurrentWeapon.magazine);
            Debug.Log("Expected: " + expected);
            Assert.True(Math.Abs(weapon.CurrentWeapon.magazine - expected) < 2.0f);
        }

        [UnityTest]
        public IEnumerator FireWeaponUntilEmptyMagazineTest()
        {
            Init();
            yield return EquipWeapon();

            // Firing 30 rounds with a fire rate of 0.1
            // Should take about 3 seconds to empty the magazine

            float start = Time.time;
            // Fire weapon until magazine is empty
            while (weapon.CurrentWeapon.magazine > 0)
            {
                yield return weapon.FireWeapon();
            }

            // Check how long it took to empty magazine
            float elapsed = Time.time - start;
            float expectedDuration = weapon.CurrentWeapon.weaponDefinition.TimeToEmptyMagazine;
            Debug.Log("Duration");
            Debug.Log("Result: " + elapsed);
            Debug.Log("Expected: " + expectedDuration);
            Assert.True(Math.Abs(elapsed - expectedDuration) < 0.5);

            // Attempt to fire weapon with empty magazine
            yield return weapon.FireWeapon();

            // Check if weapon reloaded properly after emptying magazine
            yield return new WaitUntil(() => weapon.CurrentState == Weapon.WeaponState.Idle);
            Assert.True(weapon.CurrentWeapon.magazine == weapon.CurrentWeapon.weaponDefinition.clipSize);
        }

        [UnityTest]
        public IEnumerator FireWeaponUntilEmptyMagazineWithNoReservesTest()
        {
            Init();
            yield return EquipWeapon();

            // Firing 30 rounds with a fire rate of 0.1
            // Should take about 3 seconds to empty the magazine

            float start = Time.time;
            // Fire weapon until reserves is empty
            while (weapon.CurrentWeapon.reserves + weapon.CurrentWeapon.magazine > 0)
            {
                yield return weapon.FireWeapon();
            }

            // Check how long it took to empty reserves
            float elapsed = Time.time - start;
            float expectedDuration = weapon.CurrentWeapon.weaponDefinition.TimeToEmptyEntireWeapon;
            Debug.Log("Duration");
            Debug.Log("Result: " + elapsed);
            Debug.Log("Expected: " + expectedDuration);
            Assert.True(Math.Abs(elapsed - expectedDuration) < 0.5);

            // Attempt to fire weapon with empty magazine and empty reserves
            yield return weapon.FireWeapon();

            // Check if magazine is still empty and weapon did not reload
            yield return new WaitUntil(() => weapon.CurrentState == Weapon.WeaponState.Idle);
            Assert.True(weapon.CurrentWeapon.magazine == 0 && weapon.CurrentWeapon.reserves == 0);
        }

        [UnityTest]
        public IEnumerator SwitchWeaponTest()
        {
            Init();
            yield return EquipWeapon();
        }

        // // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // // `yield return null;` to skip a frame.
        // [UnityTest]
        // public IEnumerator WeaponTestsWithEnumeratorPasses()
        // {
        //     // Use the Assert class to test conditions.
        //     // Use yield to skip a frame.
        //     Debug.Log("Unity Test");
        //     yield return null;
        // }
    }
}
