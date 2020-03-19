using Mirror;
using UnityEngine;

namespace Infection
{
    [RequireComponent(typeof(Player))]
    [RequireComponent(typeof(PlayerController))]
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInitializer : NetworkBehaviour
    {
        [SerializeField] private Behaviour[] componentsToDisable = null;
        [SerializeField] private GameObject[] gameObjectsToDisable = null;
        [SerializeField] private string remoteLayerName = "RemotePlayer";

        private void Start()
        {
            if (isLocalPlayer)
            {
                DisableObjects();
            }
            else
            {
                DisableComponents();
                AssignRemoteLayer();
            }
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            string id = GetComponent<NetworkIdentity>().netId.ToString();
            Player player = GetComponent<Player>();

            MatchManager.RegisterPlayer(id, player);
        }

        private void AssignRemoteLayer()
        {
            gameObject.layer = LayerMask.NameToLayer(remoteLayerName);
        }

        private void DisableComponents()
        {
            foreach (Behaviour component in componentsToDisable)
            {
                component.enabled = false;
            }
        }

        private void DisableObjects()
        {
            foreach (GameObject gameObject in gameObjectsToDisable)
            {
                gameObject.SetActive(false);
            }
        }

        private void OnDisable()
        {
            MatchManager.UnRegisterPlayer(transform.name);
        }
    }
}