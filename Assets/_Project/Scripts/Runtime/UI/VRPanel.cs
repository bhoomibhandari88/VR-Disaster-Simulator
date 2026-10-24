using UnityEngine;

namespace RescueSim.UI
{
    /// <summary>A world-space menu panel that appears in front of the player and stays put (never head-locked).</summary>
    public class VRPanel : MonoBehaviour
    {
        [SerializeField, Min(0.6f)] float distance = 1.7f;
        [SerializeField] float heightOffset = -0.05f;

        public bool IsVisible => gameObject.activeSelf;

        public void ShowInFrontOf(Transform head)
        {
            if (head != null)
            {
                Vector3 fwd = head.forward; fwd.y = 0f;
                if (fwd.sqrMagnitude < 0.01f) fwd = Vector3.forward;
                fwd.Normalize();
                Vector3 pos = head.position + fwd * distance;
                pos.y = head.position.y + heightOffset;
                transform.SetPositionAndRotation(pos, Quaternion.LookRotation(fwd, Vector3.up));
            }
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
