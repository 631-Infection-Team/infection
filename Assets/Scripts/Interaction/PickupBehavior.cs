using UnityEngine;

namespace Infection.Interaction
{
    [RequireComponent(typeof(Player))]
    public class PickupBehavior : MonoBehaviour
    {
        [SerializeField] private float pickupRange = 4f;
        [SerializeField] private LayerMask pickupMask = new LayerMask();

        private Camera _camera = null;
        private Player _player = null;

        private ItemPickup _raycastObj = null;

        private void Awake()
        {
            _camera = GetComponent<Player>().cam;
            _player = GetComponent<Player>();
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
                }
                // Pickup button has been pressed
                if (Input.GetButtonDown("Pickup"))
                {
                    if (pickup != null)
                    {
                        PickupItem(pickup);
                    }
                }
            }
            else
            {
                // Not pointing at any target
                if (_raycastObj != null)
                {
                    // Reset previous target material and clear reference
                    _raycastObj.Highlight(false);
                    _raycastObj = null;
                }
            }
        }

        private void PickupItem(IPickup pickup)
        {
            pickup.GrantPickup(this);
            Debug.Log("Picked up item: " + pickup);
        }
    }
}
