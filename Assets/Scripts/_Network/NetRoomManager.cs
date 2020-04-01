using UnityEngine;
using Mirror;

/*
	Documentation: https://mirror-networking.com/docs/Components/NetworkRoomManager.html
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkRoomManager.html

	See Also: NetworkManager
	Documentation: https://mirror-networking.com/docs/Components/NetworkManager.html
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
*/

/// <summary>
/// This is a specialized NetworkManager that includes a networked room.
/// The room has slots that track the joined players, and a maximum player count that is enforced.
/// It requires that the NetworkRoomPlayer component be on the room player objects.
/// NetworkRoomManager is derived from NetworkManager, and so it implements many of the virtual functions provided by the NetworkManager class.
/// </summary>
/// 

namespace Infection
{
    public class NetRoomManager : NetworkRoomManager
    {
        #region Server Callbacks

        public override void OnRoomStartServer() { }

        public override void OnRoomStartHost() { }

        public override void OnRoomStopHost() { }

        public override void OnRoomServerConnect(NetworkConnection conn) { }

        public override void OnRoomServerDisconnect(NetworkConnection conn) { }

        public override void OnRoomServerSceneChanged(string sceneName) { }

        public override GameObject OnRoomServerCreateRoomPlayer(NetworkConnection conn)
        {
            return base.OnRoomServerCreateRoomPlayer(conn);
        }

        public override GameObject OnRoomServerCreateGamePlayer(NetworkConnection conn, GameObject roomPlayer)
        {
            return base.OnRoomServerCreateGamePlayer(conn, roomPlayer);
        }

        public override void OnRoomServerAddPlayer(NetworkConnection conn)
        {
            base.OnServerAddPlayer(conn);
        }

        public override bool OnRoomServerSceneLoadedForPlayer(GameObject roomPlayer, GameObject gamePlayer)
        {
            return base.OnRoomServerSceneLoadedForPlayer(roomPlayer, gamePlayer);
        }

        public override void OnRoomServerPlayersReady()
        {
            base.OnRoomServerPlayersReady();
        }

        #endregion

        #region Client Callbacks

        public override void OnRoomClientEnter() { }

        public override void OnRoomClientExit() { }

        public override void OnRoomClientConnect(NetworkConnection conn) { }

        public override void OnRoomClientDisconnect(NetworkConnection conn) { }

        public override void OnRoomStartClient() { }

        public override void OnRoomStopClient() { }

        public override void OnRoomClientSceneChanged(NetworkConnection conn) { }

        public override void OnRoomClientAddPlayerFailed() { }

        #endregion

        #region Optional UI

        public override void OnGUI()
        {
            base.OnGUI();
        }

        #endregion
    }
}