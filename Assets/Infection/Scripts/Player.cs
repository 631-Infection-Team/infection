using UnityEngine;
using Mirror;
using Infection.Combat;
using Infection.Interaction;

namespace Infection
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerCamera))]
    [RequireComponent(typeof(PlayerAnimator))]
    [RequireComponent(typeof(PlayerMovement))]
    public class Player : NetworkBehaviour
    {
        public enum Team { SURVIVOR, INFECTED }

        [Header("Components")]
        public Weapon weapon;
        public InfectedWeapon infectedWeapon;
        public PickupBehavior pickupBehavior;
        public new Camera camera;
        public GameObject cameraContainer;
        public GameObject graphics;
        public GameObject survivorGraphics;
        public GameObject zombieGraphics;
        public GameObject hud;

        [Header("Health")]
        [SyncVar] public int health = 100;
        [SerializeField] private int maxHealth = 100;
        [SyncVar, HideInInspector] public bool isDead = false;

        [Header("Team")]
        [SyncVar] public Team team = Team.SURVIVOR;

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();

            cameraContainer.SetActive(true);
            graphics.SetActive(false);
            zombieGraphics.SetActive(false);
            hud.SetActive(true);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            string netID = GetComponent<NetworkIdentity>().netId.ToString();
            MatchManager.RegisterPlayer(netID, this);
        }

        public void Start()
        {
            SetDefaults();
        }

        public void Update()
        {
            if (!isLocalPlayer) return;
            if (isDead) CmdRespawn();
        }

        public void OnDestroy()
        {
            string netID = GetComponent<NetworkIdentity>().netId.ToString();
            MatchManager.UnRegisterPlayer(netID);

            if (isLocalPlayer)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public void Heal(int amount = 100)
        {
            health = Mathf.Clamp(health + amount, 0, maxHealth);
        }

        public void TakeDamage(int amount, uint sourceID)
        {
            health = Mathf.Clamp(health -= amount, 0, maxHealth);
            RpcOnTakeDamage();

            if (health <= 0)
            {
                Death(sourceID);
                Infect();
            }
        }

        public void Infect()
        {
            team = Team.INFECTED;
            weapon.UnequipAllWeapons();

            RpcOnInfected();
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
        }

        [Command]
        private void CmdRespawn()
        {
            SetDefaults();
            RpcOnRespawn();
        }

        [ClientRpc]
        public void RpcOnTakeDamage()
        {
            Debug.Log("Health: " + health);
        }

        [ClientRpc]
        public void RpcOnDeath()
        {
            GetComponent<CharacterController>().enabled = false;
            GetComponent<PlayerMovement>().enabled = false;
            GetComponent<PlayerAnimator>().enabled = false;
        }

        [ClientRpc]
        public void RpcOnRespawn()
        {
            GetComponent<CharacterController>().enabled = true;

            Transform spawnPoint = NetworkManager.singleton.GetStartPosition();
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;

            GetComponent<PlayerMovement>().enabled = true;
            GetComponent<PlayerAnimator>().enabled = true;
        }

        [ClientRpc]
        public void RpcOnInfected()
        {
            zombieGraphics.SetActive(true);
            survivorGraphics.SetActive(false);
            weapon.enabled = false;
            infectedWeapon.enabled = true;
            pickupBehavior.enabled = false;
        }
    }
}
