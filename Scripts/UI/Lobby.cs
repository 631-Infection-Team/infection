using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

namespace Infection.UI
{
    [DisallowMultipleComponent]
    public class Lobby : MonoBehaviour
    {
        public NetworkRoomManagerInfection networkRoomManagerInfection;
        public GameObject uiPlayerList;
        public GameObject uiPlayerListPlayer;
        public List<NetworkRoomPlayer> playerList = new List<NetworkRoomPlayer>();

        private void OnGUI()
        {
            playerList = networkRoomManagerInfection.roomSlots;

            foreach (NetworkRoomPlayer player in playerList) {
                GameObject uiPlayerInstance = Instantiate(uiPlayerListPlayer, uiPlayerList.transform);
                uiPlayerInstance.GetComponent<TextMeshProUGUI>().text = $"Player {player.index + 1}\t\t{(player.readyToBegin ? "READY" : "NOT READY")}";
            }

            playerList.Clear();
        }

        private void OnDisable()
        {
            foreach (Transform child in uiPlayerList.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
