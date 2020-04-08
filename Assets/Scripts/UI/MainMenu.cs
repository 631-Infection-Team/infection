using UnityEngine;
using FMODUnity;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Infection.UI
{
    public class MainMenu : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Transform[] disableOnChange = new Transform[12];
        private StudioEventEmitter studioEventEmitter;

        [Header("Audio")]
        [EventRef, SerializeField] private string clickEvent;
        [EventRef, SerializeField] private string hoverEvent;

        private void Start()
        {
            studioEventEmitter = GetComponent<StudioEventEmitter>();
        }

        public void SetActivePanel(Transform panel)
        {
            studioEventEmitter.Event = clickEvent;
            studioEventEmitter.Play();

            foreach (Transform child in disableOnChange)
            {
                child.gameObject.SetActive(child == panel);
            }
        }

        public void Quit()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}