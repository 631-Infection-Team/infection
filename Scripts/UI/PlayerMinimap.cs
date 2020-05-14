using Mirror;
using UnityEngine;

namespace Infection.UI
{
    public class PlayerMinimap : NetworkBehaviour
    {
        [SerializeField] private GameObject minimapMarker = null;
        [SerializeField] private Material playerMarker = null;
        // [SerializeField] private Material allyMarker = null;
        [SerializeField] private Material enemyMarker = null;

        public override void OnStartClient()
        {
            base.OnStartClient();

            minimapMarker.GetComponent<MeshRenderer>().material = isLocalPlayer ? playerMarker : enemyMarker;
            minimapMarker.SetActive(true); // This needs to be turned on for all players, not only local player
        }
    }
}
