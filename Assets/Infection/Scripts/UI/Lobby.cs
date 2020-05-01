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
        [SerializeField] private NetworkRoomManagerInfection networkRoomManagerInfection;
        [SerializeField] private GameObject uiPlayerList;
        [SerializeField] private GameObject uiPlayerListPlayer;
        [SerializeField] private List<NetworkRoomPlayer> playerList = new List<NetworkRoomPlayer>();

        private void OnGUI()
        {
            playerList = networkRoomManagerInfection.roomSlots;

            foreach (NetworkRoomPlayer player in playerList) {
                GameObject uiPlayerInstance = Instantiate(uiPlayerListPlayer, uiPlayerList.transform);
                uiPlayerInstance.GetComponent<TextMeshProUGUI>().text = "Player " + (player.index + 1);
            }

            playerList.Clear();
        }
    }
}
