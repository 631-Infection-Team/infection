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

        [Test]
        public void FiringWeaponForFiveSecondsTest()
        {
            GameObject testPlayer = new GameObject();
            Weapon weapon = testPlayer.AddComponent<Weapon>();
        }

        [Test]
        public void FireWeaponUntilEmptyMagazineTest()
        {

        }

        [Test]
        public void FireWeaponUntilEmptyMagazineWithNoReservesTest()
        {

        }

        [Test]
        public void SwitchWeaponTest()
        {

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
