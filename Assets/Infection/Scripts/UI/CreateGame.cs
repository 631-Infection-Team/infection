using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Infection.UI
{
    [DisallowMultipleComponent]
    public class CreateGame : MonoBehaviour
    {
        public GameObject playerList;

        public void AddPlayer(uint id)
        {
            Debug.Log(id);
        }

        public void RemovePlayer(uint id)
        {

        }
    }
}
