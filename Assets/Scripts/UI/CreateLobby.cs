using UnityEngine;

namespace Infection.UI
{
    public class CreateLobby : MonoBehaviour
    {
        private GameObject networkRoomManager = null;
        private NetworkRoomManagerExt roomManager = null;

        private void Start()
        {
            networkRoomManager = GameObject.Find("NetworkRoomManager");
            roomManager = networkRoomManager.GetComponent<NetworkRoomManagerExt>();
        }

        public void StartHost()
        {
            roomManager.StartHost();
        }

        public void Back()
        {
            roomManager.StopHost();
        }
    }
}