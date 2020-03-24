using Infection.Combat;
using Mirror;
using TMPro;
using UnityEngine;

namespace Infection
{
    public class HUD : NetworkBehaviour
    {
        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private TextMeshProUGUI weaponAmmoDisplay = null;
        [SerializeField] private Weapon playerWeapon = null;

        public bool isPaused;

        private void OnEnable()
        {
            playerWeapon.OnAmmoChange += UpdateAmmoDisplay;
        }

        [Client]
        public void TogglePause()
        {
            isPaused = !isPaused;
            pauseMenu.SetActive(isPaused);
        }

        [Client]
        private void UpdateAmmoDisplay()
        {
            weaponAmmoDisplay.text = $"{playerWeapon.CurrentWeapon.Magazine} | {playerWeapon.CurrentWeapon.Reserves}";
        }
    }
}
