using Mirror;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Infection
{
    public class Player : NetworkBehaviour
    {
        [SyncVar] public int index = 0;
        [SyncVar] public int health = 100;
        [SyncVar] public string username = "John Doe";

        public int kills = 0;
        public int deaths = 0;

        [SerializeField] private GameObject HUD = null;
        [SerializeField] private GameObject m_Text;
        [SerializeField] private GameObject[] disableGameObjectsOnDeath = null;
        [SerializeField] private Behaviour[] disableComponentsOnDeath = null;
        private NetRoomManager NetRoomManager = null;
        private readonly int maxHealth = 100;
        private bool isDead = false;

        private void Start()
        {
            GameObject NetworkRoomManager = GameObject.Find("Network Room Manager");

            if (NetworkRoomManager)
            {
                NetRoomManager = NetworkRoomManager.GetComponent<NetRoomManager>();
            }
            else
            {
                Debug.LogError("Could not find Network Room Manager.");
            }
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            HUD.SetActive(false);
            m_Text.GetComponent<TextMeshPro>().text = username;
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();

            HUD.SetActive(true);
        }

        public void SetDefaults()
        {
            isDead = false;
            health = maxHealth;

            foreach (GameObject obj in disableGameObjectsOnDeath)
            {
                obj.SetActive(true);
            }

            foreach (Behaviour component in disableComponentsOnDeath)
            {
                component.enabled = true;
            }

            // Create respawn effect
            // Play sound
            // Play animation
        }

        public void SetupPlayer()
        {
            if (isLocalPlayer)
            {
                gameObject.GetComponent<CameraController>().currentCamera.enabled = true;
                HUD.SetActive(true);
            }

            RpcBroadcastPlayerSetup();
        }

        [ClientRpc]
        private void RpcBroadcastPlayerSetup()
        {
            RpcSetupPlayerOnAllClients();
        }

        [ClientRpc]
        private void RpcSetupPlayerOnAllClients()
        {
            foreach (GameObject obj in disableGameObjectsOnDeath) {
                obj.SetActive(true);
            }

            foreach (Behaviour component in disableComponentsOnDeath)
            {
                component.enabled = true;
            }

            SetDefaults();
        }

        [ClientRpc]
        public void RpcTakeDamage(int dmg, string sourceID)
        {
            health = Mathf.Clamp(health - dmg, 0, 100);

            if (health <= 0)
            {
                RpcDeath(sourceID);
            }
        }

        [ClientRpc]
        public void RpcDeath(string sourceID)
        {
            isDead = true;
            deaths += 1;

            Player sourcePlayer = MatchManager.GetPlayer(sourceID);

            if (sourcePlayer)
            {
                sourcePlayer.kills += 1;
                MatchManager.instance.onPlayerKilledCallback.Invoke(username, sourcePlayer.username);
            }

            foreach (GameObject obj in disableGameObjectsOnDeath)
            {
                obj.SetActive(false);
            }

            foreach (Behaviour component in disableComponentsOnDeath)
            {
                component.enabled = false;
            }

            StartCoroutine(Respawn());
        }

        private IEnumerator Respawn()
        {
            yield return new WaitForSeconds(MatchManager.instance.respawnTime);

            Transform spawnPoint = NetRoomManager.GetStartPosition();
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;

            yield return new WaitForSeconds(0.5f);
        }
    }
}