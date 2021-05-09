// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Launcher.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking Demos
// </copyright>
// <summary>
//  Used in "PUN Basic tutorial" to handle typical game management requirements
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using Photon.Realtime;
using Photon.Pun;

namespace myTest
{


    /// <summary>
    /// Game manager.
    /// Connects and watch Photon Status, Instantiate Player
    /// Deals with quiting the room and the game
    /// Deals with level loading (outside the in room synchronization)
    /// </summary>
    public class GameManager : MonoBehaviourPunCallbacks
    {

        #region Public Fields

        static public GameManager Instance;

        #endregion

        #region Private Fields

        private GameObject instance;

        [Tooltip("The prefab to use for representing the player")]
        [SerializeField]
        private GameObject playerPrefab;

        #endregion

        #region MonoBehaviour CallBacks

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase.
        /// </summary>
        void Start()
        {
            Instance = this;
            //int count = PhotonNetwork.PlayerList.Length;

            //Debug.Log(count);
            //var PlayerUIs = FindObjectsOfType<PlayerUI>();
            //foreach (var PlayerUI in PlayerUIs)
            //{
            //    PlayerUI.UpdatePlayerCount(count.ToString());
            //}
                // in case we started this demo with the wrong scene being active, simply load the menu scene
                if (!PhotonNetwork.IsConnected)
            {
                SceneManager.LoadScene("Launcher");

                return;
            }

            if (playerPrefab == null)
            { // #Tip Never assume public properties of Components are filled up properly, always check and inform the developer of it.

                Debug.LogError("<Color=Red><b>Missing</b></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
            }
            else
            {


                if (Player1.LocalPlayerInstance == null)
                {
                    Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManagerHelper.ActiveSceneName);

                    // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                   // PhotonNetwork.Instantiate(Path.Combine("PhotonPlayer", "Survivor2 Variant"), new Vector3(-50f, 3f, -15f), Quaternion.identity, 0);
                    PhotonNetwork.Instantiate(Path.Combine("PhotonPlayer", "Survivor1(Clone)"), new Vector3(-50f, 3f, -12f), Quaternion.identity, 0);

                }
                else
                {

                    Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
                }

                PhotonNetwork.InstantiateSceneObject(Path.Combine("PhotonZombies", "Zombie1"), new Vector3(-21f, 1f, -18f), Quaternion.identity, 0);
                PhotonNetwork.InstantiateSceneObject(Path.Combine("PhotonZombies", "Zombie2"), new Vector3(-51f, 1f, -6f), Quaternion.identity, 0);
                PhotonNetwork.InstantiateSceneObject(Path.Combine("PhotonZombies", "Zombie3"), new Vector3(-30f, 1f, -41f), Quaternion.identity, 0);


            }

        }

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity on every frame.
        /// </summary>
        void Update()
        {
            // "back" button of phone equals "Escape". quit app if that's pressed
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                QuitApplication();
            }
        }

        #endregion

        #region Photon Callbacks

        /// <summary>
        /// Called when a Photon Player got connected. We need to then load a bigger scene.
        /// </summary>
        /// <param name="other">Other.</param>
        public override void OnPlayerEnteredRoom(Player other)
        {
            Debug.Log("OnPlayerEnteredRoom() " + other.NickName); // not seen if you're the player connecting

            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom

                LoadArena();
            }
        }

       /// <summary>
       /// Called when a Photon Player got disconnected. We need to load a smaller scene.
       /// </summary>
       /// <param name="other">Other.</param>
       public override void OnPlayerLeftRoom( Player other  )
       {
       	Debug.Log( "OnPlayerLeftRoom() " + other.NickName ); // seen when other disconnects

       	if ( PhotonNetwork.IsMasterClient )
       	{
       		Debug.LogFormat( "OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient ); // called before OnPlayerLeftRoom

       		LoadArena(); 
       	}
       }

        /// <summary>
        /// Called when the local player left the room. We need to load the launcher scene.
        /// </summary>
        public override void OnLeftRoom()
        {
            SceneManager.LoadScene("01_Menu 1");
        }

        #endregion

        #region Public Methods

        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }

        public void QuitApplication()
        {
            Application.Quit();
        }

        public void gameReset()
        {
            SceneManager.LoadScene("01_Menu 1");
            //if (PhotonNetwork.IsMasterClient)
            //{
            //    PhotonNetwork.LoadLevel("01_Menu 1");
            //}
        }

        #endregion

        #region Private Methods

        void LoadArena()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
            }

            Debug.LogFormat("PhotonNetwork : Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount);

            PhotonNetwork.LoadLevel("04_Downtown");
            //PhotonNetwork.LoadLevel("SampleScene");
        }

        #endregion

    }

}