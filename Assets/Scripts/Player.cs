using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Enumerations;

public class Player : NetworkBehaviour
{
    [SyncVar]
    public int index;
    [SyncVar]
    public State team;
}
