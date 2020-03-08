using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Enumerations
{
    // Menu States
    Menu_Main,
    Menu_Lobby_Join,
    Menu_Lobby_Create,
    Menu_Quit,

    // Round States
    Round_PreGame,
    Round_ActiveGame,
    Round_PostGame,
    Round_Waiting,

    // Player States
    Player_Alive,
    Player_Dead,
    Player_Spectator,
    
    // Teams
    Team_Survivor,
    Team_Infected,
    Team_Spectator
}
