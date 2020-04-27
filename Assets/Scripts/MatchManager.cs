using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infection
{
    public class MatchManager : NetworkBehaviour
    {
        public static MatchManager Instance { get; private set; }

        [Serializable]
        public class State
        {
            public string name = "";
            public int time = 0;
        }

        [Header("Settings")]
        [SerializeField] private State preGame = new State();
        [SerializeField] private State game = new State();
        [SerializeField] private State postGame = new State();

        [SyncVar] public State state;
        [SyncVar] public int currentTime = 0;
        [SyncVar] public int currentRound = 0;
        [SyncVar] public List<Player> players = new List<Player>();

        private double nextProcessingTime = 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        private void OnDestroy()
        {
            if (this == Instance)
            {
                Instance = null;
            }
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

        public override void OnStartServer()
        {
            base.OnStartServer();
            SetState(preGame);
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
            if (!isClient) return;

            HUD hud = Player.localPlayer.HUD.GetComponent<HUD>();

            if (hud != null)
            {
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
