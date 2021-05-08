using Photon.Pun;
using UnityEngine;

namespace myTest
{
    [RequireComponent(typeof(Player1))]
    public class PickupBehavior : MonoBehaviourPun
    {
        [SerializeField] private float pickupRange = 4f;
        [SerializeField] private LayerMask pickupMask = new LayerMask();

        public event LookAt OnLookAt;
        public delegate void LookAt(string name);

        private Camera _camera = null;
        private Player1 _player = null;

        private ItemPickup _raycastObj = null;

        private void Awake()
        {
            _player = GetComponent<Player1>();
            _camera = _player.camera;
        }

        private void Update()
        {
            Transform camTransform = _camera.transform;
            Ray ray = new Ray(camTransform.position, camTransform.forward);
            if (Physics.Raycast(ray, out var hit, pickupRange, pickupMask))
            {
                ItemPickup pickup = hit.transform.GetComponent<ItemPickup>();
                // Pointing at new target object not the same as previous target
                if (_raycastObj != pickup)
                {
                    // Reset previous target material
                    if (_raycastObj != null)
                    {
                        _raycastObj.Highlight(false);
                    }

                    // Store target and change material to highlight material
                    _raycastObj = pickup;
                    _raycastObj.Highlight(true);
                    OnLookAt?.Invoke(_raycastObj.ItemName);
                }
                // Pickup button has been pressed
                if (Input.GetButtonDown("Pickup"))
                {
                    if (pickup != null)
                    {
                        PickupItem(pickup);
                        ResetTarget();
                    }
                }
            }
            else
            {
                // Not pointing at any target
                if (_raycastObj != null)
                {
                    ResetTarget();
                }
            }
        }

        private void OnDisable()
        {
            ResetTarget();
        }

        // [Command]
        private void PickupItem(ItemPickup pickup)
        {
            pickup.GrantPickup(this);
        }

        private void ResetTarget()
        {
            // Reset previous target material and clear reference
            if (_raycastObj != null)
            {
                _raycastObj.Highlight(false);
                _raycastObj = null;
                OnLookAt?.Invoke(null);
            }
        }
    }
}
