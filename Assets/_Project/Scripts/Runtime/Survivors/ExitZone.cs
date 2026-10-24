using UnityEngine;

namespace RescueSim.Survivors
{
    /// <summary>Axis-aligned (in local space) evacuation volume. No physics needed, just a containment test.</summary>
    public class ExitZone : MonoBehaviour
    {
        [SerializeField] Vector3 size = new Vector3(6f, 3f, 5f);

        public Vector3 Size { get => size; set => size = value; }

        public bool Contains(Vector3 worldPosition)
        {
            Vector3 p = transform.InverseTransformPoint(worldPosition);
            Vector3 h = size * 0.5f;
            return Mathf.Abs(p.x) <= h.x && Mathf.Abs(p.y) <= h.y && Mathf.Abs(p.z) <= h.z;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.6f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, size);
        }
    }
}
