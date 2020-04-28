using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Infection
{
    [DisallowMultipleComponent]
    public class MainMenu : MonoBehaviour
    {
        public NetworkRoomManagerInfection networkRoomManagerInfection;

        public void hostChangeMap(string sceneName)
        {
            networkRoomManagerInfection.GameplayScene = sceneName;
        }
    }
}

