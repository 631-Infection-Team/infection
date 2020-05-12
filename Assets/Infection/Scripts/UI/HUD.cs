using System.Collections;
using Infection.Combat;
using Infection.Interaction;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Infection.UI
{
    public class HUD : NetworkBehaviour
    {
        public bool isPaused;

        [SerializeField] private Image crosshair = null;
        [SerializeField] private Sprite defaultCrosshair = null;
        [SerializeField] private GameObject playerPanel = null;
        // [SerializeField] private GameObject deadPanel = null;
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
        [SerializeField] private Player player = null;
        [SerializeField] private Weapon playerWeapon = null;
        [SerializeField] private InfectedWeapon infectedWeapon = null;
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

        private void Update()
        {
            healthSliderDisplay.value = player.health;
            healthValueDisplay.text = player.health.ToString();
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
            infectedWeapon.EventOnEnable += UpdateCrosshair;
            infectedWeapon.EventOnEnable += UpdateWeaponNameDisplay;
            infectedWeapon.EventOnEnable += UpdateWeaponAmmoDisplay;

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
            infectedWeapon.EventOnEnable -= UpdateCrosshair;
            infectedWeapon.EventOnEnable -= UpdateWeaponNameDisplay;
            infectedWeapon.EventOnEnable -= UpdateWeaponAmmoDisplay;

            playerPickupBehavior.OnLookAt -= UpdateInteractionMessage;
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();

            // Set weapon info at start
            UpdateWeaponAmmoDisplay();
            UpdateWeaponNameDisplay();
            UpdateCrosshair();
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

            timerDisplay.color = sec <= 10 && min <= 0 ? new Color(255, 0, 0, 0.8f) : new Color(255, 255, 255, 0.8f);
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
            if (playerWeapon.CurrentWeapon == null || infectedWeapon.isActiveAndEnabled)
            {
                magazineDisplay.text = "-";
                reservesDisplay.text = "";
                return;
            }

            magazineDisplay.text = $"{playerWeapon.CurrentWeapon.magazine}";

            // Magazine animation
            if (_magazineAnim.isPlaying)
            {
                _magazineAnim.Stop();
            }
            _magazineAnim.Play();

            if (playerWeapon.CurrentWeapon.reserves < 0)
            {
                // Show infinite ammo
                reservesDisplay.text = "∞";
                return;
            }
            reservesDisplay.text = $"{playerWeapon.CurrentWeapon.reserves}";
        }

        private void UpdateWeaponNameDisplay()
        {
            if (infectedWeapon.isActiveAndEnabled)
            {
                weaponNameDisplay.text = "INFECTED";
                return;
            }

            if (playerWeapon.CurrentWeapon != null)
            {
                weaponNameDisplay.text = $"{playerWeapon.CurrentWeapon.weaponDefinition.weaponName}";
            }
            else
            {
                weaponNameDisplay.text = "";
            }
        }

        private void UpdateCrosshair()
        {
            if (infectedWeapon.isActiveAndEnabled)
            {
                crosshair.sprite = infectedWeapon.crosshair;
                return;
            }

            if (playerWeapon.CurrentWeapon != null)
            {
                crosshair.sprite = playerWeapon.CurrentWeapon.weaponDefinition.crosshair;
            }
            else
            {
                crosshair.sprite = defaultCrosshair;
            }
        }

        private void UpdateCrosshairOpacity(float percentage)
        {
            var color = crosshair.color;
            color = new Color(color.r, color.g, color.b, Mathf.Lerp(_originalCrosshairOpacity, 0f, percentage));
            crosshair.color = color;
        }

        private void ExpandCrosshair(float percentage)
        {
            crosshair.transform.localScale = Vector3.Lerp(_originalCrosshairSize, _originalCrosshairSize * 2, percentage);
        }

        private void HandleStateChanged(Weapon.WeaponState state)
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

        private void UpdateInteractionMessage(string s)
        {
            interactionMessageDisplay.text = s != null ? $"Pickup {s}" : "";
            interactionMessageDisplay.gameObject.SetActive(s != null);
        }
    }
}
