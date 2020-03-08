using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void JoinLobby()
    {
        Debug.Log("Join Lobby");

        // For Testing
        SceneManager.LoadScene("02_Prototype");
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
        Application.Quit();
    }
}
