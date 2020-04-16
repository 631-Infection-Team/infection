using UnityEngine;

namespace Infection.Interaction
{
    public abstract class ItemPickup : MonoBehaviour, IPickup
    {
        [SerializeField] private Material highlightMaterial = null;

        private MeshRenderer _meshRenderer = null;
        private Material _baseMaterial = null;

        private void Awake()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        private void Start()
        {
            _baseMaterial = _meshRenderer.material;
        }

        public void Highlight(bool isHighlight)
        {
            _meshRenderer.material = isHighlight ? highlightMaterial : _baseMaterial;
        }

        public abstract string ItemName { get; }

        protected abstract void OnGrantPickup(PickupBehavior pickupBehavior);

        public void GrantPickup(PickupBehavior pickupBehavior)
        {
            OnGrantPickup(pickupBehavior);
            Debug.Log("Picked up item: " + ItemName);
            Destroy(gameObject);
        }
    }
}
