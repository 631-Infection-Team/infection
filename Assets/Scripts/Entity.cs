using UnityEngine;
using Mirror;

public abstract partial class Entity : NetworkBehaviour
{
    [Header("Components")]
    public Animator animator;

    [Header("State")]
    [SyncVar, SerializeField] public string state = "IDLE";

    [Header("Health")]
    [SerializeField] protected int healthMax = 100;
    [SyncVar] public int health = 100;

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
        // disable animator on server. this is a huge performance boost and
        // definitely worth one line of code (1000 monsters: 22 fps => 32 fps)
        // (!isClient because we don't want to do it in host mode either)
        // (OnStartServer doesn't know isClient yet, Start is the only option)
        if (!isClient)
        {
            animator.enabled = false;
        }
    }

    public virtual bool IsWorthUpdating()
    {
        return netIdentity.observers == null || netIdentity.observers.Count > 0;
    }

    // entity logic will be implemented with a finite state machine
    // -> we should react to every state and to every event for correctness
    // -> we keep it functional for simplicity
    // note: can still use LateUpdate for Updates that should happen in any case
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

        // update overlays in any case, except on server-only mode
        // (also update for character selection previews etc. then)
        if (!isServerOnly)
        {
            UpdateOverlays();
        }
    }

    public virtual bool CanAttack(Entity entity)
    {
        return health > 0 && entity.health > 0;
    }

    // update for server. should return the new state.
    protected abstract string UpdateServer();

    // update for client.
    protected abstract void UpdateClient();

    // can be overwritten for overlays
    protected virtual void UpdateOverlays(){}

    public bool IsMoving()
    {
        return gameObject.GetComponent<CharacterController>().velocity != Vector3.zero;
    }

    [Server]
    public void Revive()
    {
        health = Mathf.RoundToInt(healthMax);
    }

    [Server]
    public virtual void DealDamageAt(Entity entity, int amount)
    {
        entity.health -= amount;
        entity.RpcOnDamageReceived(amount);

        Utils.InvokeMany(typeof(Entity), this, "DealDamageAt_", entity, amount);
    }

    [ClientRpc]
    void RpcOnDamageReceived(int amount)
    {
        Utils.InvokeMany(typeof(Entity), this, "OnDamageReceived_", amount);
    }

    [Server]
    protected virtual void OnDeath()
    {
        Utils.InvokeMany(typeof(Entity), this, "OnDeath_");
    }
}
