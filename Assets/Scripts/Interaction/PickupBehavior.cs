using UnityEngine;

namespace Infection.Interaction
{
    [RequireComponent(typeof(Player))]
    public class PickupBehavior : MonoBehaviour
    {
        [SerializeField] private float pickupRange = 4f;
        [SerializeField] private LayerMask pickupMask = new LayerMask();

        public event LookAt OnLookAt;
        public delegate void LookAt(string name);

        private Camera _camera = null;
        private Player _player = null;

        private ItemPickup _raycastObj = null;

        private void Awake()
        {
            _player = GetComponent<Player>();
            _camera = _player.cam;
        }

        private void Update()
        {
            if (!_player.canInteract)
            {
                return;
            }

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

        private void PickupItem(IPickup pickup)
        {
            pickup.GrantPickup(this);
        }

        private void ResetTarget()
        {
            // Reset previous target material and clear reference
            _raycastObj.Highlight(false);
            _raycastObj = null;
            OnLookAt?.Invoke(null);
        }
    }
}
