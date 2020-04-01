using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Match : NetworkBehaviour
{
    [Header("Settings")]
    [SyncVar] public int timePerRound = 100;

    [SyncVar] public float currentRoundTime;

    void Start()
    {
        currentRoundTime = Time.realtimeSinceStartup;
    }

    // Update is called once per frame
    void Update()
    {
        currentRoundTime = timePerRound - Time.realtimeSinceStartup;
    }
}
