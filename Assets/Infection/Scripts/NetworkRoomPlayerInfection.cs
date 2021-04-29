using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.SceneManagement;

/*
	Documentation: https://mirror-networking.com/docs/Components/NetworkRoomPlayer.html
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkRoomPlayer.html
*/

/// <summary>
/// This component works in conjunction with the NetworkRoomManager to make up the multiplayer room system.
/// The RoomPrefab object of the NetworkRoomManager must have this component on it.
/// This component holds basic room player data required for the room to function.
/// Game specific data for room players can be put in other components on the RoomPrefab or in scripts derived from NetworkRoomPlayer.
/// </summary>
///

namespace Infection
{
    public class NetworkRoomPlayerInfection : NetworkBehaviour
    {
        //public TMP_InputField userName = null;

        //#region Room Client Callbacks

        ///// <summary>
        ///// This is a hook that is invoked on all player objects when entering the room.
        ///// <para>Note: isLocalPlayer is not guaranteed to be set until OnStartLocalPlayer is called.</para>
        ///// </summary>
        ////public override void OnClientEnterRoom()
        ////{
        ////    base.OnClientEnterRoom();
        ////}

        /////// <summary>
        /////// This is a hook that is invoked on all player objects when exiting the room.
        /////// </summary>
        ////public override void OnClientExitRoom()
        ////{
        ////    base.OnClientExitRoom();
        ////}

        /////// <summary>
        /////// This is a hook that is invoked on clients when a RoomPlayer switches between ready or not ready.
        /////// <para>This function is called when the a client player calls SendReadyToBeginMessage() or SendNotReadyToBeginMessage().</para>
        /////// </summary>
        /////// <param name="readyState">Whether the player is ready or not.</param>
        ////public override void OnClientReady(bool readyState)
        ////{
        ////    base.OnClientReady(readyState);
        ////}

        //#endregion

        //#region Optional UI

        //public override void OnGUI()
        //{
        //    if (!showRoomGUI)
        //        return;

        //    NetworkRoomManagerInfection room = NetworkManager.singleton as NetworkRoomManagerInfection;
        //    if (room)
        //    {
        //        if (!NetworkManager.IsSceneActive(room.RoomScene))
        //            return;

        //        DrawPlayerReadyState();
        //        DrawPlayerReadyButton();
        //    }
        //}

        //void DrawPlayerReadyState()
        //{
        //    NetworkRoomManagerInfection room = NetworkManager.singleton as NetworkRoomManagerInfection;
        //    Rect rect = room.GetWorldRect();

        //    GUILayout.BeginArea(new Rect(rect.x, rect.y + 100f + (index * rect.height / 8f), rect.width, rect.height / 8f));
        //    {
        //        var style = new GUIStyle
        //        {
        //            fontSize = 40,
        //            normal =
        //            {
        //                textColor = Color.white,
        //            }
        //        };
        //        GUILayout.Label($"Player {index + 1}{(isLocalPlayer ? " (You)" : "")}", style);

        //        var readyStyle = new GUIStyle
        //        {
        //            fontSize = 20
        //        };
        //        if (readyToBegin)
        //        {
        //            readyStyle.normal.textColor = Color.green;
        //            GUILayout.Label("Ready", readyStyle);
        //        }
        //        else
        //        {
        //            readyStyle.normal.textColor = Color.red;
        //            GUILayout.Label("Not Ready", readyStyle);
        //        }

        //        var removeStyle = new GUIStyle(GUI.skin.box)
        //        {
        //            fixedHeight = 40f,
        //            fontSize = 24,
        //            alignment = TextAnchor.MiddleCenter,
        //            normal =
        //            {
        //                textColor = Color.black,
        //                background = Texture2D.whiteTexture
        //            }
        //        };
        //        if (isServer && index > 0 || isServerOnly)
        //        {
        //            GUILayout.BeginArea(new Rect(350f, 15f, 150f, 40f));
        //            {
        //                if (GUILayout.Button("Kick", removeStyle))
        //                {
        //                    // This button only shows on the Host for all players other than the Host
        //                    // Host and Players can't remove themselves (stop the client instead)
        //                    // Host can kick a Player this way.
        //                    GetComponent<NetworkIdentity>().connectionToClient.Disconnect();
        //                }
        //            }
        //            GUILayout.EndArea();
        //        }
        //    }
        //    GUILayout.EndArea();
        //}

        //void DrawPlayerReadyButton()
        //{
        //    NetworkRoomManagerInfection room = NetworkManager.singleton as NetworkRoomManagerInfection;
        //    Rect rect = room.GetWorldRect();
        //    if (PhotonNetwork.IsConnected() && photonView.IsMine)
        //    {
        //        GUILayout.BeginArea(new Rect(rect.x + 350f, rect.y + 120f + (index * rect.height / 8f), 150f, 40f));
        //        {
        //            var style = new GUIStyle(GUI.skin.box)
        //            {
        //                fixedHeight = 40f,
        //                fontSize = 30,
        //                alignment = TextAnchor.MiddleCenter,
        //                normal =
        //                {
        //                    textColor = Color.black,
        //                    background = Texture2D.whiteTexture
        //                }
        //            };
        //            if (readyToBegin)
        //            {
        //                if (GUILayout.Button("Cancel", style))
        //                {
        //                    CmdChangeReadyState(false);
        //                }
        //            }
        //            else
        //            {
        //                if (GUILayout.Button("Ready", style))
        //                {
        //                    CmdChangeReadyState(true);
        //                }
        //            }
        //        }
        //        GUILayout.EndArea();
        //    }
        //}

        //#endregion
    }
}
