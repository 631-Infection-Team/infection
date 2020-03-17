using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Infection
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;
        public delegate void OnPlayerKilledCallback(string player, string source);
        public OnPlayerKilledCallback onPlayerKilledCallback;

        private const string PLAYER_ID_PREFIX = "Player_";
        private static Dictionary<string, Player> players = new Dictionary<string, Player>();

        private void Awake()
        {
            instance = this;
        }

        public static void RegisterPlayer(string id, Player player)
        {
            string playerID = PLAYER_ID_PREFIX + id;

            players.Add(playerID, player);
            player.transform.name = playerID;
        }

        public static void UnRegisterPlayer(string playerID)
        {
            players.Remove(playerID);
        }

        public static Player GetPlayer(string playerID)
        {
            return players[playerID];
        }

        public static Player[] GetAllPlayers()
        {
            return players.Values.ToArray();
        }
    }
}