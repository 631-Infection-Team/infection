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
        public enum Team { SURVIVOR, INFECTED }

        [Header("Components")]
        public new Camera camera;
        public GameObject cameraContainer;
        public GameObject graphics;

        [Header("Health")]
        [SyncVar(hook = nameof(OnHealthChanged))] public int health = 100;
        [SerializeField] private int maxHealth = 100;
        [SyncVar, HideInInspector] public bool isDead = false;

        [Header("Team")]
        [SyncVar] public Team team = Team.SURVIVOR;

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            cameraContainer.SetActive(true);
            graphics.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void Start()
        {
            SetDefaults();
        }

        public void Heal(int amount = 100)
        {
            if (isDead) return;

            health = Mathf.Clamp(health + amount, 0, maxHealth);
        }

        public void TakeDamage(int amount, uint sourceID)
        {
            if (isDead) return;

            health = Mathf.Clamp(health -= amount, 0, maxHealth);
            RpcOnTakeDamage();

            if (health <= 0) Death(sourceID);
        }

        public void Infect()
        {
            if (team == Team.INFECTED) return;

            team = Team.INFECTED;
        }

        private void SetDefaults()
        {
            health = maxHealth;
            isDead = false;
        }

        private void Death(uint sourceID)
        {
            health = 0;
            isDead = true;

            RpcOnDeath();
            Respawn();
        }

        private void Respawn()
        {
            RpcOnRespawn();
            SetDefaults();
        }

        private void OnHealthChanged(int oldValue, int newValue)
        {
            if (isDead) return;

            Debug.Log("Old Health: " + oldValue + ", New Health: " + newValue);
        }

        [ClientRpc]
        public void RpcOnTakeDamage()
        {
            // Debug.Log("Health: " + health);
        }

        [ClientRpc]
        public void RpcOnDeath()
        {
            GetComponent<CharacterController>().enabled = false;
            Infect();
        }

        [ClientRpc]
        public void RpcOnRespawn()
        {
            Transform spawnPoint = NetworkManager.singleton.GetStartPosition();
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;

            GetComponent<CharacterController>().enabled = true;
        }
    }
}
