using Infection.Combat;
using Mirror;
using TMPro;
using UnityEngine;

namespace Infection
{
    public class HUD : NetworkBehaviour
    {
        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private TextMeshProUGUI weaponNameDisplay = null;
        [SerializeField] private TextMeshProUGUI weaponAmmoDisplay = null;
        [SerializeField] private Weapon playerWeapon = null;

        public bool isPaused;

        private void Start()
        {
            // Set weapon info at start
            UpdateWeaponAmmoDisplay();
            UpdateWeaponNameDisplay();
        }

        private void OnEnable()
        {
            playerWeapon.OnAmmoChange += UpdateWeaponAmmoDisplay;
            playerWeapon.OnWeaponChange += UpdateWeaponNameDisplay;
        }

        private void OnDisable()
        {
            playerWeapon.OnAmmoChange -= UpdateWeaponAmmoDisplay;
            playerWeapon.OnWeaponChange -= UpdateWeaponNameDisplay;
        }

        [Client]
        public void TogglePause()
        {
            isPaused = !isPaused;
            pauseMenu.SetActive(isPaused);
        }

        [Client]
        private void UpdateWeaponAmmoDisplay()
        {
            weaponAmmoDisplay.text = $"{playerWeapon.CurrentWeapon.Magazine} | {playerWeapon.CurrentWeapon.Reserves}";
        }

        [Client]
        private void UpdateWeaponNameDisplay()
        {
            weaponNameDisplay.text = $"{playerWeapon.CurrentWeapon.WeaponDefinition.WeaponName}";
        }
    }
}
