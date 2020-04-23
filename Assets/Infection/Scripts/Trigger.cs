using UnityEngine;

namespace Infection
{
    public enum Type { Kill, Fire }

    [RequireComponent(typeof(BoxCollider))]
    public class Trigger : MonoBehaviour
    {
        public Type type = Type.Kill;
        [SerializeField] private Color gizmoColor = new Color(0, 1, 1, 0.25f);
        [SerializeField] private Color gizmoWireColor = new Color(1, 1, 1, 0.8f);

        private void Start()
        {
            BoxCollider collider = GetComponent<BoxCollider>();
            collider.isTrigger = true;
        }

        private void OnDrawGizmos()
        {
            BoxCollider collider = GetComponent<BoxCollider>();
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            Gizmos.color = gizmoColor;
            Gizmos.DrawCube(collider.center, collider.size);
            Gizmos.color = gizmoWireColor;
            Gizmos.DrawWireCube(collider.center, collider.size);
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}

