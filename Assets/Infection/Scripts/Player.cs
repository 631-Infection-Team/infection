using UnityEngine;
using Mirror;
using System.Collections;

namespace Infection
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerCamera))]
    public class Player : NetworkBehaviour
    {
        [Header("Components")]
        public new Camera camera;
        public GameObject cameraContainer;
        public GameObject graphics;

        [Header("Health")]
        [SyncVar] public int health = 100;
        [SerializeField] private int maxHealth = 100;
        [SyncVar, HideInInspector] public bool isDead = false;

        public override void OnStartLocalPlayer()
        {
            cameraContainer.SetActive(true);
            graphics.SetActive(false);
        }

        public void Start()
        {
            SetDefaults();
        }

        public void Heal(int amount = 100)
        {
            health = Mathf.Clamp(health + amount, 0, maxHealth);
        }

        [ClientRpc]
        public void RpcOnTakeDamage(int amount, string sourceID)
        {
            if (isDead) return;

            health = Mathf.Clamp(health -= amount, 0, maxHealth);

            if (health <= 0) OnDeath(sourceID);
        }

        private void OnDeath(string sourceID)
        {
            GetComponent<CharacterController>().enabled = false;
            isDead = true;

            Debug.Log(gameObject.name + " was killed by " + sourceID);

            StartCoroutine(Respawn());
        }

        private void SetDefaults()
        {
            GetComponent<CharacterController>().enabled = true;
            health = maxHealth;
            isDead = false;
        }

        private IEnumerator Respawn()
        {
            yield return new WaitForSeconds(3);

            Transform spawnPoint = NetworkManager.singleton.GetStartPosition();
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;

            yield return new WaitForSeconds(0.1f);

            SetDefaults();
        }
    }
}
