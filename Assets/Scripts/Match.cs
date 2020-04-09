using Mirror;
using System.Collections;
using UnityEngine;

namespace Infection
{
    public class Match : NetworkBehaviour
    {
        [System.Serializable]
        private class State
        {
            public string name;
            public int time;
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
            SetState(game);
            StartCoroutine(Tick());
        }

        private void SetState(State state)
        {
            this.state = state;
            currentTime = state.time;
        }

        [Server]
        private IEnumerator Tick()
        {
            if (!isServer) yield break;

            while (currentTime > 0)
            {
                yield return new WaitForSecondsRealtime(1f);

                currentTime -= 1;
                RpcTick();
            }
            
            // Round over goto next state
            // Make sure to increase current round
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
