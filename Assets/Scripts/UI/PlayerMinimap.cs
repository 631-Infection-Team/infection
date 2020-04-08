using Mirror;
using UnityEngine;

namespace _UI
{
    public class PlayerMinimap : NetworkBehaviour
    {
        [SerializeField] private GameObject minimapMarker = null;
        [SerializeField] private Material playerMarker = null;
        // [SerializeField] private Material allyMarker = null;
        [SerializeField] private Material enemyMarker = null;

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();

            minimapMarker.SetActive(true);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (isLocalPlayer)
            {
                minimapMarker.GetComponent<MeshRenderer>().material = playerMarker;
            }
            else
            {
                minimapMarker.GetComponent<MeshRenderer>().material = enemyMarker;
            }
        }
    }
}
