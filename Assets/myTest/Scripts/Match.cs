using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;

namespace myTest
{
    public class Match : MonoBehaviourPunCallbacks
    {
        public GameObject RoundEnd;
        public GameObject ParameterReset;
        public PhotonView photonView;

        [Serializable]
        public class State
        {
            public string name = "";
            public int time = 0;
        }

        [Header("Settings")]
        public State preGame = new State();
        public State game = new State();
        public State postGame = new State();

        [SerializeField]
        private GameObject DeathUI;

        private State _state;
        ExitGames.Client.Photon.Hashtable CustomValue;
        double timerIncrementValue;
        double startTime;
        bool startTimer = false;
        bool tracker = false;

        private void Start()
        {
            DeathUI.SetActive(false);
            if (PhotonNetwork.IsMasterClient)
            {
                CustomValue = new ExitGames.Client.Photon.Hashtable();
                startTime = PhotonNetwork.Time;
                startTimer = true;
                CustomValue["StartTime"] = startTime;
                PhotonNetwork.CurrentRoom.SetCustomProperties(CustomValue);
            }
            else
            {
                //StartCoroutine(ExampleCoroutine());
            }
            SetState(preGame);
            //InvokeRepeating(nameof(Tick), 1f, 1f);
        }
        IEnumerator ExampleCoroutine()
        {
            //Print the time of when the function is first called.
            Debug.Log("Started Coroutine at timestamp : " + Time.time);

            //yield on a new YieldInstruction that waits for 5 seconds.
            yield return new WaitForSeconds(5);

            //After we have waited 5 seconds print the time again.
            Debug.Log("Finished Coroutine at timestamp : " + Time.time);
            tracker = true;
        }

        void startTimerOther()
        {
            if (startTimer != true)
            {
                startTime = double.Parse(PhotonNetwork.CurrentRoom.CustomProperties["StartTime"].ToString());
                startTimer = true;
                PhotonNetwork.CurrentRoom.CustomProperties["StartTime"] = null;
                //Debug.Log(PhotonNetwork.CurrentRoom.CustomProperties["StartTime"]);
            }
        }
        void Update()
        {
            if (!PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.CustomProperties["StartTime"] != null )
            {
                startTimerOther();
            }

            if (!startTimer) return;

            timerIncrementValue = PhotonNetwork.Time - startTime;
            var PlayerUIs = FindObjectsOfType<PlayerUI>();
            foreach (var PlayerUI in PlayerUIs)
            {
                int playerNum = GameObject.FindGameObjectsWithTag("Player").Length;
                PlayerUI.UpdatePlayerCount(playerNum);
                PlayerUI.UpdateTimer((float)(_state.time + 1 - timerIncrementValue));
                PlayerUI.UpdateState(GetRoundInfo());
                if (_state == postGame && GameObject.FindGameObjectsWithTag("Player").Length == 1)
                {
                    string WinnerUserName = GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<PhotonView>().Owner.NickName;
                    PlayerUI.UpdateRoundMessage("GAME OVER: " + WinnerUserName + " has won!");
                }
                else if (_state == postGame)
                {
                    PlayerUI.UpdateRoundMessage("GAME OVER: Time is Up");
                }
                else
                {
                    PlayerUI.UpdateRoundMessage("");
                }
            }


            if ((_state.time + 1 - timerIncrementValue) > 1)
            {
              // if (_state == game && GameObject.FindGameObjectsWithTag("Player").Length == 1)
              // {
              //     SetState(postGame);
              // }
            }
            else
            {
                if (_state == preGame && GameObject.FindGameObjectsWithTag("Player").Length == 1)
                {
                    Debug.Log("TO POST");
                 //   SetState(postGame);
                }
                else if (_state == preGame)
                {
                    Debug.Log("TO GAME");
                    SetState(game);
                }
                else if (_state == game)
                {
                    Debug.Log("TO POST");
                  //  SetState(postGame);
                }
                else if (_state == postGame)
                {
                    GameObject.Find("GameManager").GetComponent<GameManager>().LeaveRoom();
                    GameObject.Find("GameManager").GetComponent<GameManager>().gameReset();
                    //GameObject.Find("GameManager").GetComponent<GameManager>()
                    //Debug.Log("Reset");
                    //SetState(preGame);
                    
                }
            }

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

        //[Server]
        private void SetState(State state)
        {
            _state = state;
            if (PhotonNetwork.IsMasterClient)
            {
                startTime = PhotonNetwork.Time;
                CustomValue["StartTime"] = startTime;
                PhotonNetwork.CurrentRoom.SetCustomProperties(CustomValue);
                //Debug.Log(PhotonNetwork.CurrentRoom.CustomProperties["StartTime"]);
            }
            if (!PhotonNetwork.IsMasterClient)
            {
                startTime = PhotonNetwork.Time;
                startTimer = false;
            }
        }

        public void activeDeathUI()
        {
            DeathUI.SetActive(true);
        }



        //private void Tick()
        //{
        //    if (_currentTime > 0)
        //    {
        //        if (_state == game && GameObject.FindGameObjectsWithTag("Player").Length==1)
        //        {
        //            SetState(postGame);
        //            RoundEnd.SetActive(true);
        //        }

        //        _currentTime -= 1;
        //        RpcTick();

        //        if (_state == game && _currentTime == 1)
        //        {
        //            SetState(postGame);
        //            RoundEnd.SetActive(true);
        //        }
        //    }
        //    else
        //    {
        //        if (_state == preGame)
        //        {
        //            ParameterReset.SetActive(false);
        //            SetState(game);
        //            _currentRound += 1;
        //        }
        //        else if (_state == game)
        //        {
        //            RoundEnd.SetActive(true);
        //            SetState(postGame);
        //        }
        //        else if (_state == postGame)
        //        {
        //            RoundEnd.SetActive(false);
        //            ParameterReset.SetActive(true);
        //            SetState(preGame);
        //        }
        //    }
        //}

        ////[ClientRpc]
        //public void RpcTick()
        //{
        //    // HUD hud = Player.localPlayer.HUD.GetComponent<HUD>();
        //    //
        //    // hud.UpdateTimer(_currentTime);
        //    // hud.UpdateRound(GetRoundInfo());
        //    var PlayerUIs = FindObjectsOfType<PlayerUI>();
        //    foreach (var PlayerUI in PlayerUIs)
        //    {
        //        int playerNum = GameObject.FindGameObjectsWithTag("Player").Length;
        //        PlayerUI.UpdatePlayerCount(playerNum);
        //        PlayerUI.UpdateTimer(_currentTime);
        //        PlayerUI.UpdateState(GetRoundInfo());
        //        // Win message during post round
        //        if (_state == postGame)
        //        {
        //            PlayerUI.UpdateRoundMessage("GAME OVER");
        //        }
        //        else
        //        {
        //            PlayerUI.UpdateRoundMessage("");
        //        }
        //    }
        //}
    }
}