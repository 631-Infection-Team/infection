using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Infection
{
    public class PlayerGame : NetworkBehaviour
    {
        [SyncVar] public int index;

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
    }
}