using Mirror;
using UnityEngine;

namespace Infection
{
    public class HUD : NetworkBehaviour
    {
        [SerializeField] private GameObject pauseMenu;

        public bool isPaused;

        [Client]
        public void TogglePause()
        {
            isPaused = !isPaused;
            pauseMenu.SetActive(isPaused);
        }
    }
}
