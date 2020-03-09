using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Enumerations;

public class LobbyManager : NetworkRoomManager
{
    public override bool OnRoomServerSceneLoadedForPlayer(GameObject roomPlayer, GameObject gamePlayer)
    {
        Player player = gamePlayer.GetComponent<Player>();
        player.index = roomPlayer.GetComponent<NetworkRoomPlayer>().index;
        player.team = State.Team_Survivor;

        return true;
    }

    public override void OnRoomServerPlayersReady()
    {
        base.OnRoomServerPlayersReady();
    }
}
