using System;
using System.Collections;
using Infection.Combat;
using Infection.Interaction;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Infection
{
    public class HUD : MonoBehaviour
    {
        public bool isPaused;

        [SerializeField] private Image crosshair = null;
        [SerializeField] private Sprite defaultCrosshair = null;
        [SerializeField] private GameObject playerPanel = null;
        [SerializeField] private GameObject deadPanel = null;
        [SerializeField] private GameObject pausePanel = null;
        [SerializeField] private TextMeshProUGUI healthValueDisplay = null;
        [SerializeField] private TextMeshProUGUI statusMessageDisplay = null;
        [SerializeField] private TextMeshProUGUI alertMessageDisplay = null;
        [SerializeField] private TextMeshProUGUI interactionMessageDisplay = null;
        [SerializeField] private TextMeshProUGUI weaponNameDisplay = null;
        [SerializeField] private TextMeshProUGUI magazineDisplay = null;
        [SerializeField] private TextMeshProUGUI reservesDisplay = null;
        [SerializeField] private TextMeshProUGUI timerDisplay = null;
        [SerializeField] private TextMeshProUGUI roundDisplay = null;
        [SerializeField] private Slider healthSliderDisplay = null;
        [SerializeField] private Weapon playerWeapon = null;
        [SerializeField] private PickupBehavior playerPickupBehavior = null;

        private float _originalCrosshairOpacity = 0f;
        private Vector3 _originalCrosshairSize;

        // HUD animations
        private Animation _healthTextAnim = null;
        private Animation _magazineAnim = null;
        private Animation _timerAnim = null;

        private void Awake()
        {
            _healthTextAnim = healthValueDisplay.gameObject.GetComponent<Animation>();
            _magazineAnim = magazineDisplay.gameObject.GetComponent<Animation>();
            _timerAnim = timerDisplay.gameObject.GetComponent<Animation>();
        }

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
            timerDisplay.text = "";
            roundDisplay.text = "";

            interactionMessageDisplay.gameObject.SetActive(false);

            // Set weapon info at start
            UpdateWeaponAmmoDisplay();
            UpdateWeaponNameDisplay();
            UpdateCrosshair();

            playerWeapon.EventOnAmmoChange += UpdateWeaponAmmoDisplay;
            playerWeapon.EventOnWeaponChange += UpdateWeaponNameDisplay;
            playerWeapon.EventOnWeaponChange += UpdateCrosshair;
            playerWeapon.EventOnStateChange += HandleStateChanged;
            playerWeapon.EventOnAimingChange += UpdateCrosshairOpacity;
            playerWeapon.EventOnRecoil += ExpandCrosshair;
            playerWeapon.EventOnAlert += UpdateAlertMessage;

            playerPickupBehavior.OnLookAt += UpdateInteractionMessage;
        }

        private void OnDisable()
        {
            playerWeapon.EventOnAmmoChange -= UpdateWeaponAmmoDisplay;
            playerWeapon.EventOnWeaponChange -= UpdateWeaponNameDisplay;
            playerWeapon.EventOnWeaponChange -= UpdateCrosshair;
            playerWeapon.EventOnStateChange -= HandleStateChanged;
            playerWeapon.EventOnAimingChange -= UpdateCrosshairOpacity;
            playerWeapon.EventOnRecoil -= ExpandCrosshair;
            playerWeapon.EventOnAlert -= UpdateAlertMessage;

            playerPickupBehavior.OnLookAt -= UpdateInteractionMessage;
        }

        public void SetPaused(bool state)
        {
            isPaused = state;
            playerPanel.SetActive(!isPaused);
            pausePanel.SetActive(isPaused);
        }

        public void UpdateTimer(float timeLeft)
        {
            int min = Mathf.FloorToInt(timeLeft / 60);
            int sec = Mathf.FloorToInt(timeLeft % 60);

            timerDisplay.color = sec <= 10 ? new Color(255, 0, 0, 0.8f) : new Color(255, 255, 255, 0.8f);
            timerDisplay.text = min.ToString("00") + ":" + sec.ToString("00");

            // Round Timer animation
            _timerAnim.Play();
        }

        public void UpdateRound(string info)
        {
            roundDisplay.text = info;
        }

        public void SetHealthMax(int healthMax)
        {
            healthSliderDisplay.maxValue = healthMax;
        }

        public void SetHealth(int health)
        {
            healthValueDisplay.text = health.ToString();
            healthSliderDisplay.value = Mathf.Clamp(health, 0, healthSliderDisplay.maxValue);
            healthValueDisplay.color = health <= (healthSliderDisplay.maxValue / 4) ? Color.red : Color.white;

            // Health Text animation
            if (_healthTextAnim.isPlaying)
            {
                _healthTextAnim.Stop();
            }
            _healthTextAnim.Play();
        }

        private IEnumerator UpdateAlertMessage(string message, float duration)
        {
            alertMessageDisplay.text = message;
            yield return new WaitForSeconds(duration);
            alertMessageDisplay.text = "";
        }

        private void UpdateWeaponAmmoDisplay()
        {
            // No weapon
            if (playerWeapon.CurrentWeapon == null)
            {
                magazineDisplay.text = "-";
                reservesDisplay.text = "";
                return;
            }

            magazineDisplay.text = $"{playerWeapon.CurrentWeapon.Magazine}";

            // Magazine animation
            if (_magazineAnim.isPlaying)
            {
                _magazineAnim.Stop();
            }
            _magazineAnim.Play();

            if (playerWeapon.CurrentWeapon.Reserves < 0)
            {
                // Show infinite ammo
                reservesDisplay.text = "∞";
                return;
            }
            reservesDisplay.text = $"{playerWeapon.CurrentWeapon.Reserves}";
        }

        private void UpdateWeaponNameDisplay()
        {
            if (playerWeapon.CurrentWeapon != null)
            {
                weaponNameDisplay.text = $"{playerWeapon.CurrentWeapon.WeaponDefinition.WeaponName}";
            }
            else
            {
                weaponNameDisplay.text = "No Weapon";
            }
        }

        private void UpdateCrosshair()
        {
            if (playerWeapon.CurrentWeapon != null)
            {
                crosshair.sprite = playerWeapon.CurrentWeapon.WeaponDefinition.Crosshair;
            }
            else
            {
                crosshair.sprite = defaultCrosshair;
            }
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

        private void UpdateInteractionMessage(string s)
        {
            interactionMessageDisplay.text = s != null ? $"Pickup {s}" : "";
            interactionMessageDisplay.gameObject.SetActive(s != null);
        }
    }
}
