using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainMenu : MonoBehaviour
{
    public static MainMenu Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void JoinLobby()
    {
        Debug.Log("Join Lobby");

        // For Testing
        SceneManager.LoadScene("03_Prototype");
    }

    public void CreateLobby()
    {
        Debug.Log("Create Lobby");
    }

    public void Settings()
    {
        Debug.Log("Open Settings");
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
