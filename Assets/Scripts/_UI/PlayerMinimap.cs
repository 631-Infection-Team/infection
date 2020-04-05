using Mirror;
using UnityEngine;

namespace _UI
{
    public class PlayerMinimap : NetworkBehaviour
    {
        [SerializeField] private GameObject minimapMarker = null;
        [SerializeField] private Material playerMarker = null;
        [SerializeField] private Material allyMarker = null;
        [SerializeField] private Material enemyMarker = null;

        private void Start()
        {
            if (isLocalPlayer)
            {
                minimapMarker.GetComponent<MeshRenderer>().material = playerMarker;
            }
            else
            {
                minimapMarker.GetComponent<MeshRenderer>().material = enemyMarker;
            }

            minimapMarker.SetActive(true);
        }
    }
}
