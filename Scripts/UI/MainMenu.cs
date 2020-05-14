using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Infection.UI
{
    [DisallowMultipleComponent]
    public class MainMenu : MonoBehaviour
    {
        public NetworkRoomManagerInfection networkRoomManagerInfection;

        public void HostChangeMap(string sceneName)
        {
            networkRoomManagerInfection.GameplayScene = sceneName;
        }

        public void QuitApplication()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
}

