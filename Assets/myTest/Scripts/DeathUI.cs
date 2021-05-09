using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

namespace myTest
{
    public class DeathUI : MonoBehaviour
    {
        // Start is called before the first frame update
        private Animation _timerAnim = null;

        [SerializeField] private TextMeshProUGUI timerDisplay = null;
        [SerializeField] private TextMeshProUGUI gameState = null;
        [SerializeField] private TextMeshProUGUI endMessage = null;
        [SerializeField] private TextMeshProUGUI roundMessage = null;

        [SerializeField] private TextMeshProUGUI playerCount = null;
        [SerializeField] private TextMeshProUGUI Player1 = null;
        [SerializeField] private TextMeshProUGUI Player2 = null;
        [SerializeField] private TextMeshProUGUI Player3 = null;
        [SerializeField] private TextMeshProUGUI Player4 = null;
        void Start()
        {

        }

        void Awake()
        {
            _timerAnim = timerDisplay.gameObject.GetComponent<Animation>();
        }

        private void OnEnable()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void UpdateState(string info)
        {
            gameState.text = info;
        }

        public void UpdateEndMessage(string message)
        {
            endMessage.text = message;
        }

        public void UpdateRoundInfo(string message)
        {
            roundMessage.text = message;
        }
        public void UpdatePlayerCount(int number)
        {
            if (number == 1)
            {
                playerCount.text = "1 Player Left";
            }
            else
            {
                playerCount.text = number.ToString() + " Players Left";
            }
        }

        public void UpdateAlivePlayers(string[] playerNames)
        {
            if(playerNames.Length > 0)
            {
                Player1.text = playerNames[0];
            }
            if (playerNames.Length > 1)
            {
                Player2.text = playerNames[1];
            }
            if (playerNames.Length > 2)
            {
                Player3.text = playerNames[2];
            }
            if (playerNames.Length > 3)
            {
                Player4.text = playerNames[3];
            }
        }

        public void UpdateTimer(float timeLeft)
        {
            int min = Mathf.FloorToInt(timeLeft / 60);
            int sec = Mathf.FloorToInt(timeLeft % 60);

            timerDisplay.color = sec <= 10 && min <= 0 ? new Color(255, 0, 0, 0.8f) : new Color(255, 255, 255, 0.8f);
            timerDisplay.text = min.ToString("00") + ":" + sec.ToString("00");

            // Round Timer animation
            _timerAnim.Play();
        }
    }
}
