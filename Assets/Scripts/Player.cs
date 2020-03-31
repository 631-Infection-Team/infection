using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Player : Entity
{
    [HideInInspector] public string account = "";
    public static Player localPlayer;
    public Camera cam;

    protected override void Awake()
    {
        base.Awake();

        Utils.InvokeMany(typeof(Player), this, "Awake_");
    }

    public override void OnStartLocalPlayer()
    {
        localPlayer = this;
        cam = Camera.main;

        Utils.InvokeMany(typeof(Player), this, "OnStartLocalPlayer_");
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        Utils.InvokeMany(typeof(Player), this, "OnStartServer_");
    }

    protected override void Start()
    {
        if (!isServer && !isClient) return;

        base.Start();

        Utils.InvokeMany(typeof(Player), this, "Start_");
    }

    void LateUpdate()
    {
        // pass parameters to animation state machine
        // => passing the states directly is the most reliable way to avoid all
        //    kinds of glitches like movement sliding, attack twitching, etc.
        // => make sure to import all looping animations like idle/run/attack
        //    with 'loop time' enabled, otherwise the client might only play it
        //    once
        // => MOVING state is set to local IsMovement result directly. otherwise
        //    we would see animation latencies for rubberband movement if we
        //    have to wait for MOVING state to be received from the server
        // => MOVING checks if !CASTING because there is a case in UpdateMOVING
        //    -> SkillRequest where we still slide to the final position (which
        //    is good), but we should show the casting animation then.
        // => skill names are assumed to be boolean parameters in animator
        //    so we don't need to worry about an animation number etc.

        if (isClient) // no need for animations on the server
        {
            // now pass parameters after any possible rebinds
            foreach (Animator anim in GetComponentsInChildren<Animator>())
            {
                anim.SetBool("MOVING", IsMoving());
                anim.SetBool("DEAD", state == "DEAD");
            }
        }

        Utils.InvokeMany(typeof(Player), this, "LateUpdate_");
    }

    void OnDestroy()
    {
        if (!isServer && !isClient) return;

        if (isLocalPlayer)
        {
            localPlayer = null;
        }

        Utils.InvokeMany(typeof(Player), this, "OnDestroy_");
    }

    // finite state machine events
    bool EventDied()
    {
        return health <= 0;
    }

    bool EventMoveStart()
    {
        return state != "MOVING" && IsMoving(); // only fire when started moving
    }

    bool EventMoveEnd()
    {
        return state == "MOVING" && !IsMoving(); // only fire when stopped moving
    }

    [Command]
    public void CmdRespawn() { respawnRequested = true; }
    bool respawnRequested;
    bool EventRespawn()
    {
        bool result = respawnRequested;
        respawnRequested = false; // reset
        return result;
    }

    // finite state machine - server
    [Server]
    string UpdateServer_IDLE()
    {
        if (EventDied())
        {
            OnDeath();
            return "DEAD";
        }
        if (EventMoveStart())
        {
            return "MOVING";
        }
        if (EventMoveEnd()) { }
        if (EventRespawn()) { }

        return "IDLE";
    }

    [Server]
    string UpdateServer_MOVING()
    {
        if (EventDied())
        {
            OnDeath();
            return "DEAD";
        }
        if (EventMoveEnd())
        {
            return "IDLE";
        }
        if (EventMoveStart()) { }
        if (EventRespawn()) { }

        return "MOVING";
    }

    [Server]
    string UpdateServer_DEAD()
    {
        if (EventRespawn())
        {
            gameObject.transform.position = new Vector3(0, 0, 0);

            Revive();
            return "IDLE";
        }

        if (EventMoveStart()) { }
        if (EventMoveEnd()) { }
        if (EventDied()) { }

        return "DEAD";
    }

    [Server]
    protected override string UpdateServer()
    {
        if (state == "IDLE") return UpdateServer_IDLE();
        if (state == "MOVING") return UpdateServer_MOVING();
        if (state == "DEAD") return UpdateServer_DEAD();
        Debug.LogError("invalid state:" + state);
        return "IDLE";
    }

    // finite state machine - client
    [Client]
    protected override void UpdateClient()
    {
        if (state == "IDLE" || state == "MOVING")
        {
            if (isLocalPlayer)
            {
                WASDHandling();
            }
        }
        else if (state == "DEAD") { }
        else Debug.LogError("invalid state:" + state);

        Utils.InvokeMany(typeof(Player), this, "UpdateClient_");
    }

    protected override void UpdateOverlays()
    {
        base.UpdateOverlays();
    }

    [Server]
    protected override void OnDeath()
    {
        base.OnDeath();

        Utils.InvokeMany(typeof(Player), this, "OnDeath_");
    }

    public override bool CanAttack(Entity entity)
    {
        return base.CanAttack(entity) && entity is Player;
    }

    [Client]
    void WASDHandling()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // create input vector, normalize in case of diagonal movement
        Vector3 input = new Vector3(horizontal, 0, vertical);
        if (input.magnitude > 1) input = input.normalized;

        Vector3 angles = cam.transform.rotation.eulerAngles;
        Quaternion rotation = Quaternion.Euler(angles);
        Vector3 direction = rotation * input;

        // draw direction for debugging
        Debug.DrawLine(transform.position, transform.position + direction, Color.green, 0, false);

        gameObject.GetComponent<CharacterController>().Move(direction);
    }
}
