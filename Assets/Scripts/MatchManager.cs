using Mirror;
using System.Collections;
using UnityEngine;

namespace Infection
{
    public class MatchManager : NetworkBehaviour
    {
        [System.Serializable]
        public class State
        {
            public string name = "";
            public int time = 0;
        }

        [Header("Settings")]
        [SerializeField] public State preGame = new State();
        [SerializeField] public State game = new State();
        [SerializeField] public State postGame = new State();

        [SyncVar] public State state;
        [SyncVar] public int currentTime = 0;
        [SyncVar] public int currentRound = 0;

        private double nextProcessingTime = 0;

        public override void OnStartServer()
        {
            base.OnStartServer();

            SetState(preGame);
        }

        private void Update()
        {
            if (!isServer) return;

            nextProcessingTime += Time.deltaTime;
            if (nextProcessingTime >= 1)
            {
                nextProcessingTime = 0;

                if (currentTime > 0)
                {
                    RpcTick();
                    currentTime -= 1;
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
        }

        [Server]
        private void SetState(State state)
        {
            if (!isServer) return;

            this.state = state;
            currentTime = state.time;
        }

        [ClientRpc]
        public void RpcTick()
        {
            if (isClient)
            {
                HUD hud = Player.localPlayer.HUD.GetComponent<HUD>();

                if (state == game)
                {
                    hud.UpdateRound("Round " + currentRound);
                }
                else
                {
                    hud.UpdateRound(state.name);
                }

                hud.UpdateTimer(currentTime);
            }
        }
    }
}
