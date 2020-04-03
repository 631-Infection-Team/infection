using UnityEngine;
using Mirror;

namespace Infection
{
    public abstract partial class Entity : NetworkBehaviour
    {
        [Header("State")]
        [SyncVar, SerializeField] public string state = "IDLE";

        [Header("Health")]
        [SerializeField] protected int healthMax = 100;
        [SyncVar] public int health = 100;

        [Header("Components")]
        public Animator animator;

        protected virtual void Awake()
        {
            Utils.InvokeMany(typeof(Entity), this, "Awake_");
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            if (health <= 0)
            {
                state = "DEAD";
            }

            Utils.InvokeMany(typeof(Entity), this, "OnStartServer_");
        }

        protected virtual void Start()
        {
            if (!isClient)
            {
                animator.enabled = false;
            }
        }

        public virtual bool IsWorthUpdating()
        {
            return netIdentity.observers == null || netIdentity.observers.Count > 0;
        }

        void Update()
        {
            if (IsWorthUpdating())
            {
                if (isClient)
                {
                    UpdateClient();
                }

                if (isServer)
                {
                    state = UpdateServer();
                }

                Utils.InvokeMany(typeof(Entity), this, "Update_");
            }
        }

        public virtual bool CanAttack(Entity entity)
        {
            return health > 0 && entity.health > 0;
        }

        protected abstract string UpdateServer();

        protected abstract void UpdateClient();

        [ClientRpc]
        public void RpcOnRespawn()
        {
            gameObject.transform.position = NetRoomManager.netRoomManager.GetStartPosition().position;
            health = healthMax;
        }

        [Server]
        public virtual void DealDamageTo(Entity entity, float amount)
        {
            if (CanAttack(entity))
            {
                entity.health -= Mathf.RoundToInt(amount);
                entity.RpcOnDamageReceived(Mathf.RoundToInt(amount));

                Utils.InvokeMany(typeof(Entity), this, "DealDamageAt_", entity, amount);

                Debug.Log(gameObject.name + " dealt damage to " + entity.name);
            }
        }

        [ClientRpc]
        void RpcOnDamageReceived(int amount)
        {
            Utils.InvokeMany(typeof(Entity), this, "OnDamageReceived_", amount);
        }

        [Server]
        protected virtual void OnDeath()
        {
            Debug.Log(gameObject.name + " died.", gameObject);
            Utils.InvokeMany(typeof(Entity), this, "OnDeath_");
        }

        [Server]
        protected virtual void OnRespawn()
        {
            Utils.InvokeMany(typeof(Entity), this, "OnRespawn_");
        }
    }
}