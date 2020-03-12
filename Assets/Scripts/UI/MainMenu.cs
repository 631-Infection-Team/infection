using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainMenu : MonoBehaviour
{
    private GameObject networkRoomManager = null;
    private NetworkRoomManagerExt roomManager = null;

    private void Start()
    {
        networkRoomManager = GameObject.Find("NetworkRoomManager");
        roomManager = networkRoomManager.GetComponent<NetworkRoomManagerExt>();
    }

    public void JoinLobby()
    {
        roomManager.StartClient();
    }

    public void CreateLobby()
    {
        roomManager.StartHost();
        roomManager.ServerChangeScene("02_Lobby");
    }

    public void Settings()
    {
        // For Testing
        roomManager.ServerChangeScene("03_Prototype");
    }

    public void Quit()
    {
        #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
