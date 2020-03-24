using Infection.Combat;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Infection
{
    public class HUD : NetworkBehaviour
    {
        [SerializeField] private Image crosshair = null;
        [SerializeField] private GameObject pauseMenu = null;
        [SerializeField] private TextMeshProUGUI statusMessageDisplay = null;
        [SerializeField] private TextMeshProUGUI weaponNameDisplay = null;
        [SerializeField] private TextMeshProUGUI magazineDisplay = null;
        [SerializeField] private TextMeshProUGUI reservesDisplay = null;
        [SerializeField] private Weapon playerWeapon = null;

        public bool isPaused;

        private void Start()
        {
            // Clear status message at start
            UpdateStatusMessage("");

            // Set weapon info at start
            UpdateWeaponAmmoDisplay();
            UpdateWeaponNameDisplay();
            UpdateCrosshair();
        }

        private void Update()
        {
            // Update status message display to reflect weapon state
            string statusMessage = "";
            switch (playerWeapon.CurrentState)
            {
                case Weapon.WeaponState.Reloading:
                    statusMessage = "Reloading";
                    break;
                case Weapon.WeaponState.Switching:
                    statusMessage = "Switching";
                    break;
            }
            UpdateStatusMessage(statusMessage);
        }

        private void OnEnable()
        {
            playerWeapon.OnAmmoChange += UpdateWeaponAmmoDisplay;
            playerWeapon.OnWeaponChange += UpdateWeaponNameDisplay;
            playerWeapon.OnWeaponChange += UpdateCrosshair;
        }

        private void OnDisable()
        {
            playerWeapon.OnAmmoChange -= UpdateWeaponAmmoDisplay;
            playerWeapon.OnWeaponChange -= UpdateWeaponNameDisplay;
            playerWeapon.OnWeaponChange -= UpdateCrosshair;
        }

        [Client]
        public void TogglePause()
        {
            isPaused = !isPaused;
            pauseMenu.SetActive(isPaused);
        }

        [Client]
        private void UpdateStatusMessage(string message)
        {
            statusMessageDisplay.text = message;
        }

        [Client]
        private void UpdateWeaponAmmoDisplay()
        {
            magazineDisplay.text = $"{playerWeapon.CurrentWeapon.Magazine}";
            reservesDisplay.text = $"{playerWeapon.CurrentWeapon.Reserves}";
        }

        [Client]
        private void UpdateWeaponNameDisplay()
        {
            weaponNameDisplay.text = $"{playerWeapon.CurrentWeapon.WeaponDefinition.WeaponName}";
        }

        [Client]
        private void UpdateCrosshair()
        {
            crosshair.sprite = playerWeapon.CurrentWeapon.WeaponDefinition.Crosshair;
        }
    }
}
