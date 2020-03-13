using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Infection
{
    public class MainMenu : MonoBehaviour
    {
        private GameObject networkRoomManager = null;
        private NetworkRoomManagerExt roomManager = null;

        [SerializeField] private Panel activePanel;
        [SerializeField] private Queue<Panel> panelStack = new Queue<Panel>();
        private enum Panel
        {
            MAIN,
            PLAY,
            CREATE,
            SETTINGS,
            QUIT
        }

        [Header("Panels")]
        [SerializeField] private GameObject main = null;
        [SerializeField] private GameObject play = null;
        [SerializeField] private GameObject create = null;
        [SerializeField] private GameObject settings = null;
        [SerializeField] private GameObject quit = null;

        private void Start()
        {
            networkRoomManager = GameObject.Find("NetworkRoomManager");
            roomManager = networkRoomManager.GetComponent<NetworkRoomManagerExt>();

            SetActivePanel(Panel.MAIN);
        }

        private void SetActivePanel(Panel panel)
        {
            activePanel = panel;
            panelStack.Enqueue(panel);

            if (panel == Panel.MAIN)
            {
                main.SetActive(true);

                play.SetActive(false);
                create.SetActive(false);
                settings.SetActive(false);
                quit.SetActive(false);
            }
            else if (panel == Panel.PLAY)
            {
                play.SetActive(true);
            }
            else if (panel == Panel.CREATE)
            {
                create.SetActive(true);
            }
            else if (panel == Panel.SETTINGS)
            {
                settings.SetActive(true);
            }
            else if (panel == Panel.QUIT)
            {
                quit.SetActive(true);
            }
        }

        public void GoBack()
        {
            if (panelStack.Count > 0)
            {
                SetActivePanel(panelStack.Dequeue());
            }
        }

        public void Play()
        {
            SetActivePanel(Panel.PLAY);
            roomManager.StartClient();
        }

        public void Create()
        {
            SetActivePanel(Panel.CREATE);
            roomManager.StartHost();
        }

        public void Settings()
        {
            SetActivePanel(Panel.SETTINGS);
        }

        public void Quit()
        {
            // Show a panel asking if you're sure you want to quit.
            SetActivePanel(Panel.QUIT);

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}