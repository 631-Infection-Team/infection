using UnityEngine;

namespace Infection.UI
{
    public class CreateLobby : MonoBehaviour
    {
        [SerializeField] private GameObject networkRoomManager = null;
        private NetRoomManager roomManager = null;

        private void Start()
        {
            if (networkRoomManager)
            {
                roomManager = networkRoomManager.GetComponent<NetRoomManager>();
            }
            else
            {
                Debug.LogError("Network Room Manager not assigned in Inspector.");
            }

        }

        public void StartHost()
        {
            if (roomManager)
            {
                roomManager.StartHost();
            }
        }

        public void Back()
        {
            if (roomManager)
            {
                roomManager.StopHost();
            }
        }
    }
}