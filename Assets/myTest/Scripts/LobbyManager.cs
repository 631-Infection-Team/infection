using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using TMPro;

public class LobbyManager : MonoBehaviourPunCallbacks
{
  public GameObject nickNamePrefab;
  public Transform nickNameList;
  public TextMeshProUGUI nickNameField;
  public TextMeshProUGUI lobbyNameField;

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
          PhotonNetwork.LocalPlayer.NickName = playerName;
          RoomOptions roomOptions = new RoomOptions();
          TypedLobby typedLobby = new TypedLobby(lobbyName, LobbyType.Default);
          PhotonNetwork.JoinOrCreateRoom(lobbyName, roomOptions, typedLobby);

      }

    }

    //go through list of players to add to the waiting room UI
    public void AddPlayerToList(Player player){
      GameObject nameEntry = Instantiate(nickNamePrefab, nickNameList);
      //get component that is child of the name entry (from nickname prefab) that is a TextMeshProUGUI object
      TextMeshProUGUI nameField = nameEntry.GetComponentInChildren<TextMeshProUGUI>();
      nameField.SetText(player.NickName);
    }
    public override void OnJoinedRoom(){
      //loop through players
      foreach(Player player in PhotonNetwork.CurrentRoom.Players.Values){
        AddPlayerToList(player);
      }
    }
    public override void OnPlayerEnteredRoom(Player player){
      AddPlayerToList(player);
    }


    //public void load


}
