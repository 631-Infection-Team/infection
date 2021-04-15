using System;
using Mirror;
using Infection.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Infection
{
    public class Match : NetworkBehaviour
    {
        public GameObject RoundEnd;
        public GameObject ParameterReset;
        

        [Serializable]
        public class State
        {
            [SyncVar] public string name = "";
            [SyncVar] public int time = 0;
        }

        [Header("Settings")]
        public State preGame = new State();
        public State game = new State();
        public State postGame = new State();

        [SyncVar] private State _state;
        [SyncVar] private int _currentTime = 0;
        [SyncVar] private int _currentRound = 0;

        private void Start()
        {
            if (!isServer) return;

            SetState(preGame);
            InvokeRepeating(nameof(Tick), 1f, 1f);
        }

        private string GetRoundInfo()
        {
            if (_state == preGame)
            {
                return "Warm Up\nGame is over when only one is left standing!";
            }
            if (_state == game)
            {
                return "Kill all enemies, Human or Zombie";
            }
            if (_state == postGame)
            {
                return "Game Over";
            }

            return _state.name;
        }

        [Server]
        private void SetState(State state)
        {
            _state = state;
            _currentTime = state.time + 1;
        }

      

        [Server]
        private void Tick()
        {
            if (_currentTime > 0)
            {
                _currentTime -= 1;
                RpcTick();

                if (_state == game && _currentTime == 1)
                {
                    SetState(postGame);
                    MatchManager.FreezeAllPlayers();
                    RoundEnd.SetActive (true);
                }
            }
            else
            {
                if (_state == preGame)
                {
                    ParameterReset.SetActive (false);
                    SetState(game);
                    _currentRound += 1;
                    //MatchManager.InfectRandom();
                }
                else if (_state == game)
                {
                    RoundEnd.SetActive (true);
                    SetState(postGame);
                    MatchManager.FreezeAllPlayers(); 
                }
                else if (_state == postGame)
                {
                    RoundEnd.SetActive (false);
                    ParameterReset.SetActive (true);
                    SetState(preGame);
                    MatchManager.ResetAllPlayers();
                }
            }
        }

        [ClientRpc]
        public void RpcTick()
        {
            // HUD hud = Player.localPlayer.HUD.GetComponent<HUD>();
            //
            // hud.UpdateTimer(_currentTime);
            // hud.UpdateRound(GetRoundInfo());

            var huds = FindObjectsOfType<HUD>();
            foreach (var hud in huds)
            {
                hud.UpdateTimer(_currentTime);
                hud.UpdateRound(GetRoundInfo());

                // Win message during post round
                if (_state == postGame)
                {
                    hud.UpdateRoundMessage("GAME OVER: Player 1 Wins!");
                }
                else
                {
                    hud.UpdateRoundMessage("");
                }
            }
        }
    }
}
