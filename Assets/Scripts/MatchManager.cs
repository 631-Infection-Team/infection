using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Infection
{
    public class MatchManager : MonoBehaviour
    {
        public static MatchManager instance;
        public float respawnTime = 3f;
        public delegate void OnPlayerKilledCallback(string player, string source);
        public OnPlayerKilledCallback onPlayerKilledCallback;

        private static Dictionary<string, Player> players = new Dictionary<string, Player>();

        private void Awake()
        {
            instance = this;
        }

        public static void RegisterPlayer(string id, Player player)
        {
            players.Add("player_" + id, player);
            player.transform.name = "player_" + id;
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