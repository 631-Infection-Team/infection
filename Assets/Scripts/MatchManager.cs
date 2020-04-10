using Mirror;
using System.Collections;
using UnityEngine;

namespace Infection
{
    public class MatchManager : NetworkBehaviour
    {
        [System.Serializable]
        private class State
        {
            public string name = "";
            public int time = 0;
        }

        [Header("Settings")]
        [SerializeField] private State preGame = new State();
        [SerializeField] private State game = new State();
        [SerializeField] private State postGame = new State();

        private State state;
        private int currentTime = 0;
        private int currentRound = 1;
        private void Start()
        {
            if (!isServer) return;

            SetState(game);
            InvokeRepeating("Tick", 1f, 1f);
        }

        [Server]
        private void SetState(State state)
        {
            this.state = state;
            currentTime = state.time + 1;
        }

        [Server]
        private void Tick()
        {
            if (currentTime > 0)
            {
                currentTime -= 1;
                RpcTick();
            }
            else
            {
                if (state == preGame)
                {
                    SetState(game);
                    currentRound += 1;
                }
                else if (state == game)
                {
                    SetState(postGame);
                }
                else if (state == postGame)
                {
                    SetState(preGame);
                }
            }
        }

        [ClientRpc]
        public void RpcTick()
        {
            HUD hud = Player.localPlayer.HUD.GetComponent<HUD>();

            hud.UpdateTimer(currentTime);
            hud.UpdateRound(currentRound);
        }
    }
}
