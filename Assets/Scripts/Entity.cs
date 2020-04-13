using UnityEngine;
using Mirror;

namespace Infection
{
    public abstract partial class Entity : NetworkBehaviour
    {
        [Header("Health")]
        [SyncVar] public int health = 100;
        public int healthMax = 100;

        protected virtual bool CanAttack(Entity entity)
        {
            return health > 0 && entity.health > 0;
        }

        [ClientRpc]
        public abstract void RpcOnDamageReceived(int amount);
    }
}
