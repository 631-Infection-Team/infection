using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using TMPro;

public class LobbyManager : MonoBehaviour
{
  public TextMeshProUGUI nickNameField;
  public TextMeshProUGUI lobbyNameField;
    //stop welcoming me please

    // Start is called before the first frame update
    void Start()
    {
      PlayerPrefs.DeleteAll();
      //sets up the networking, connects to Photon using ap id
      PhotonNetwork.ConnectUsingSettings();
    }
    //method when join room clicked
    public void JoinRoom(){

      if (PhotonNetwork.IsConnected)
      {
          string playerName = nickNameField.text;
          string lobbyName = lobbyNameField.text;
          PhotonNetwork.LocalPlayer.NickName = playerName; //1
          RoomOptions roomOptions = new RoomOptions(); //2
          TypedLobby typedLobby = new TypedLobby(lobbyName, LobbyType.Default); //3
          PhotonNetwork.JoinOrCreateRoom(lobbyName, roomOptions, typedLobby); //4
      }

    }

    //public void load


}
