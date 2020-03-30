using System.Collections;
using System.Collections.Generic;
using Infection.Combat;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Infection
{
    public class HUD : MonoBehaviour
    {
        [SerializeField] private Image crosshair = null;
        [SerializeField] private GameObject pauseMenu = null;
        [SerializeField] private TextMeshProUGUI statusMessageDisplay = null;
        [SerializeField] private TextMeshProUGUI alertMessageDisplay = null;
        [SerializeField] private TextMeshProUGUI weaponNameDisplay = null;
        [SerializeField] private TextMeshProUGUI magazineDisplay = null;
        [SerializeField] private TextMeshProUGUI reservesDisplay = null;
        [SerializeField] private Weapon playerWeapon = null;

        public bool isPaused;

        private void Start()
        {
            // Clear status and alert messages at start
            statusMessageDisplay.text = "";
            alertMessageDisplay.text = "";

            // Set weapon info at start
            UpdateWeaponAmmoDisplay();
            UpdateWeaponNameDisplay();
            UpdateCrosshair();
        }

        private void OnGUI()
        {
            string statusMessage = "";
            // Update status message display to reflect weapon state
            switch (playerWeapon.CurrentState)
            {
                case Weapon.WeaponState.Reloading:
                    statusMessage = "Reloading";
                    break;
                case Weapon.WeaponState.Switching:
                    statusMessage = "Switching";
                    break;
            }
            statusMessageDisplay.text = statusMessage;
        }

        private void OnEnable()
        {
            playerWeapon.OnAmmoChange += UpdateWeaponAmmoDisplay;
            playerWeapon.OnWeaponChange += UpdateWeaponNameDisplay;
            playerWeapon.OnWeaponChange += UpdateCrosshair;
            playerWeapon.OnAlertEvent += UpdateAlertMessage;
        }

        private void OnDisable()
        {
            playerWeapon.OnAmmoChange -= UpdateWeaponAmmoDisplay;
            playerWeapon.OnWeaponChange -= UpdateWeaponNameDisplay;
            playerWeapon.OnWeaponChange -= UpdateCrosshair;
            playerWeapon.OnAlertEvent -= UpdateAlertMessage;
        }

        public void TogglePause()
        {
            isPaused = !isPaused;
            pauseMenu.SetActive(isPaused);
        }

        private IEnumerator UpdateAlertMessage(string message, float duration)
        {
            alertMessageDisplay.text = message;
            yield return new WaitForSeconds(duration);
            alertMessageDisplay.text = "";
        }

        private void UpdateWeaponAmmoDisplay()
        {
            magazineDisplay.text = $"{playerWeapon.CurrentWeapon.Magazine}";
            reservesDisplay.text = $"{playerWeapon.CurrentWeapon.Reserves}";
        }

        private void UpdateWeaponNameDisplay()
        {
            weaponNameDisplay.text = $"{playerWeapon.CurrentWeapon.WeaponDefinition.WeaponName}";
        }

        private void UpdateCrosshair()
        {
            crosshair.sprite = playerWeapon.CurrentWeapon.WeaponDefinition.Crosshair;
        }
    }
}
