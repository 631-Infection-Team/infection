using Mirror;
using UnityEngine;

namespace Infection
{
    public class Player : NetworkBehaviour
    {
        [SyncVar] public int index;
        [SyncVar] public int maxHealth = 100;
        [SyncVar] public int health = 100;

        public bool Alive()
        {
            return health > 0;
        }

        public void TakeDamage(int dmg)
        {
            health = Mathf.Clamp(health - dmg, 0, 100);
        }
    }
}