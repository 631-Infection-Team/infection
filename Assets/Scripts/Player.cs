using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class Player : Entity
{
    [Header("Components")]
    public static Player localPlayer;
    public Camera cam;

    [Header("Movement")]
    public float walkSpeed = 8f;
    public float runSpeed = 12f;

    private CharacterController characterController;
    private float verticalLook;
    private float horizontalLook;
    protected override void Awake()
    {
        base.Awake();

        Utils.InvokeMany(typeof(Player), this, "Awake_");
    }

    public override void OnStartLocalPlayer()
    {
        localPlayer = this;

        verticalLook = 0.0f;
        horizontalLook = gameObject.transform.localEulerAngles.y;

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

        characterController = gameObject.GetComponent<CharacterController>();

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

        if (isClient) // no need for animations on the server
        {
            // now pass parameters after any possible rebinds
            foreach (Animator anim in GetComponentsInChildren<Animator>())
            {
                anim.SetFloat("Head_Vertical_f", -(verticalLook / 90f));
                anim.SetFloat("Body_Vertical_f", -(verticalLook / 90f) / 2);

                anim.SetFloat("Speed_f", 0.0f);
                anim.SetBool("Death_b", state == "DEAD");
                anim.SetBool("Grounded", true);
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
                CameraHandler();
                MovementHandler();
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
    void MovementHandler()
    {
        float inputHorizontal = Input.GetAxis("Horizontal");
        float inputVertical = Input.GetAxis("Vertical");
        bool inputRun = Input.GetButton("Run");

        Vector3 move = new Vector3(inputHorizontal, 0, inputVertical);
        if (move.magnitude > 1) move = move.normalized;

        float actualSpeed = inputRun ? runSpeed : walkSpeed;
        float calcSpeed = actualSpeed;
        move = move * calcSpeed * Time.deltaTime;
        move = transform.TransformDirection(move);

        characterController.Move(move);

        // draw direction for debugging
        // Debug.DrawLine(transform.position, transform.position + direction, Color.green, 0, false);
    }

    [Client]
    void CameraHandler()
    {
        float lookY = Input.GetAxis("Mouse Y");
        float lookX = Input.GetAxis("Mouse X");

        verticalLook -= lookY;
        if (verticalLook > 90f) verticalLook = 90f;
        if (verticalLook < -90f) verticalLook = -90f;

        Vector3 currentAngles = cam.transform.localEulerAngles;
        currentAngles.x = verticalLook;
        // currentAngles.z = Vector3.Dot(characterController.velocity, -transform.right);

        cam.transform.localEulerAngles = currentAngles;

        horizontalLook += lookX;
        if (horizontalLook > 360) horizontalLook -= 360.0f;
        if (horizontalLook < 0) horizontalLook += 360.0f;

        currentAngles = transform.localEulerAngles;
        currentAngles.y = horizontalLook;
        transform.localEulerAngles = currentAngles;
    }
}
