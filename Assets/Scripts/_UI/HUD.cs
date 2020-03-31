using System.Collections;
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

        private void OnGUI()
        {

        }

        private void OnEnable()
        {
            // Clear status and alert messages at start
            statusMessageDisplay.text = "";
            alertMessageDisplay.text = "";

            // Set weapon info at start
            UpdateWeaponAmmoDisplay();
            UpdateWeaponNameDisplay();
            UpdateCrosshair();

            playerWeapon.OnAmmoChange += UpdateWeaponAmmoDisplay;
            playerWeapon.OnWeaponChange += UpdateWeaponNameDisplay;
            playerWeapon.OnWeaponChange += UpdateCrosshair;
            playerWeapon.OnStateChange += HandleStateChanged;
            playerWeapon.OnAlertEvent += UpdateAlertMessage;
        }

        private void OnDisable()
        {
            playerWeapon.OnAmmoChange -= UpdateWeaponAmmoDisplay;
            playerWeapon.OnWeaponChange -= UpdateWeaponNameDisplay;
            playerWeapon.OnWeaponChange -= UpdateCrosshair;
            playerWeapon.OnStateChange -= HandleStateChanged;
            playerWeapon.OnAlertEvent -= UpdateAlertMessage;
        }

        public void TogglePause()
        {
            isPaused = !isPaused;
            pauseMenu.SetActive(isPaused);

            Player.localPlayer.canMove = !isPaused;
        }

        private IEnumerator UpdateAlertMessage(string message, float duration)
        {
            alertMessageDisplay.text = message;
            yield return new WaitForSeconds(duration);
            alertMessageDisplay.text = "";
        }

        private void UpdateStatusMessage(Weapon.WeaponState state)
        {
            // Blank message for idle and firing
            string statusMessage = "";
            // Update status message display to reflect weapon state
            switch (state)
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

        private void HandleStateChanged(object sender, Weapon.StateChangedEventArgs e)
        {
            UpdateStatusMessage(e.State);
        }
    }
}
