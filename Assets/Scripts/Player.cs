using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Infection
{
    public class Player : NetworkBehaviour
    {
        [SyncVar] public int index;
        [SyncVar] public int health = 100;

        private void Start()
        {
            if (isLocalPlayer)
            {
                // Hide model for LocalPlayer. Show them a different model.
                SkinnedMeshRenderer[] skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();

                foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
                {
                    skinnedMeshRenderer.enabled = false;
                }
            }
        }

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