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

        private float _originalCrosshairOpacity = 0f;
        private Vector3 _originalCrosshairSize;

        private void Start()
        {
            _originalCrosshairOpacity = crosshair.color.a;
            _originalCrosshairSize = crosshair.transform.localScale;
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
            playerWeapon.OnAimingChange += UpdateCrosshairOpacity;
            playerWeapon.OnRecoil += ExpandCrosshair;
            playerWeapon.OnAlertEvent += UpdateAlertMessage;
        }

        private void OnDisable()
        {
            playerWeapon.OnAmmoChange -= UpdateWeaponAmmoDisplay;
            playerWeapon.OnWeaponChange -= UpdateWeaponNameDisplay;
            playerWeapon.OnWeaponChange -= UpdateCrosshair;
            playerWeapon.OnStateChange -= HandleStateChanged;
            playerWeapon.OnAimingChange -= UpdateCrosshairOpacity;
            playerWeapon.OnRecoil -= ExpandCrosshair;
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

        private void UpdateCrosshairOpacity(object sender, Weapon.PercentageEventArgs e)
        {
            var color = crosshair.color;
            color = new Color(color.r, color.g, color.b, Mathf.Lerp(_originalCrosshairOpacity, 0f, e.Percentage));
            crosshair.color = color;
        }

        private void ExpandCrosshair(object sender, Weapon.PercentageEventArgs e)
        {
            crosshair.transform.localScale = Vector3.Lerp(_originalCrosshairSize, _originalCrosshairSize * 2, e.Percentage);
        }

        private void HandleStateChanged(object sender, Weapon.StateChangedEventArgs e)
        {
            // Blank message for idle and firing
            string statusMessage = "";
            // Update status message display to reflect weapon state
            switch (e.State)
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
    }
}
